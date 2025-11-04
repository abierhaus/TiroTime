# TiroTime Constitution

## Purpose
This constitution establishes the foundational principles, standards, and requirements for the TiroTime application—a modern .NET web application built using Domain-Driven Design (DDD) principles. These guidelines ensure code quality, maintainability, performance, and an exceptional user experience.

---

## 1. Code Quality Principles

### 1.1 Clean Code Standards
- **Readability First**: Code must be self-documenting with clear, intention-revealing names for classes, methods, and variables
- **Single Responsibility**: Each class, method, and module must have one clear responsibility
- **DRY Principle**: Eliminate code duplication through proper abstractions and reusable components
- **SOLID Principles**: All code must adhere to SOLID principles appropriate for its layer
- **Explicit Over Implicit**: Favor explicit dependencies and clear data flows over magic behavior

### 1.2 Domain-Driven Design Architecture
- **Ubiquitous Language**: Use consistent domain terminology across code, documentation, and communication
- **Bounded Contexts**: Maintain clear boundaries between different domain contexts
- **Aggregate Roots**: Enforce invariants and business rules through properly designed aggregates
- **Domain Events**: Use domain events to communicate state changes and decouple bounded contexts
- **Layered Architecture**: Maintain strict separation between Domain, Application, Infrastructure, and Presentation layers

#### Layer Responsibilities
- **Domain Layer**: Contains business logic, entities, value objects, domain events, and domain services. No dependencies on other layers
- **Application Layer**: Orchestrates domain logic, handles use cases, and defines application services. Depends only on Domain layer
- **Infrastructure Layer**: Implements technical concerns (persistence, external services, messaging). Depends on Application and Domain layers
- **Presentation Layer**: Handles HTTP concerns, validation, and response formatting. Depends on Application layer

### 1.3 Code Review Standards
- All code changes require peer review before merging
- Reviewers must verify adherence to architectural principles and design patterns
- Security vulnerabilities must be identified and addressed
- Performance implications must be considered and documented
- Breaking changes require explicit discussion and approval

### 1.4 Documentation Requirements
- **XML Documentation**: All public APIs must include XML documentation comments
- **Architecture Decision Records (ADRs)**: Significant architectural decisions must be documented
- **README Files**: Each project/module must have a README explaining its purpose and structure
- **Domain Documentation**: Complex business rules and domain logic must be documented inline

---

## 2. Testing Standards

### 2.1 Test Pyramid
- **Unit Tests**: 70% coverage - Fast, isolated tests for domain logic and business rules
- **Integration Tests**: 20% coverage - Tests for infrastructure components and external dependencies
- **End-to-End Tests**: 10% coverage - Critical user journeys and workflows

### 2.2 Unit Testing Requirements
- **Domain Layer**: 90%+ code coverage for all entities, value objects, and domain services
- **Application Layer**: 85%+ coverage for application services and command/query handlers
- **Test Independence**: Tests must be isolated and runnable in any order
- **Naming Convention**: Use descriptive test names following `MethodName_Scenario_ExpectedBehavior` pattern
- **AAA Pattern**: Structure tests using Arrange-Act-Assert pattern

### 2.3 Integration Testing Requirements
- **Repository Tests**: Verify all database operations against a real database (in-memory or containerized)
- **API Tests**: Test all HTTP endpoints with various scenarios (success, validation errors, authorization)
- **External Services**: Use testcontainers or mock servers for third-party integrations
- **Transaction Behavior**: Verify proper transaction handling and rollback scenarios

### 2.4 Testing Tools & Frameworks
- **Unit Testing**: xUnit as primary test framework
- **Mocking**: NSubstitute or Moq for test doubles
- **Assertions**: FluentAssertions for readable assertions
- **Test Data**: Bogus for generating realistic test data
- **Coverage**: Coverlet for code coverage analysis (minimum 80% overall)

### 2.5 Test Quality Standards
- Tests must be maintainable and refactored alongside production code
- Flaky tests must be fixed immediately or removed
- Test execution time must remain fast (full suite < 5 minutes)
- No tests should be skipped or ignored in the main branch

---

## 3. User Experience Consistency

### 3.1 API Design Principles
- **RESTful Conventions**: Follow REST best practices for resource naming and HTTP verb usage
- **Consistent Response Format**: All API responses must use a standardized structure
- **Error Handling**: Return consistent error responses with appropriate HTTP status codes and problem details (RFC 7807)
- **Versioning**: Use URI versioning (e.g., `/api/v1/`) for breaking changes
- **HATEOAS**: Include hypermedia links where appropriate for discoverability

### 3.2 Response Standards
```json
Success Response:
{
  "data": { /* resource data */ },
  "metadata": {
    "timestamp": "ISO-8601",
    "version": "1.0"
  }
}

Error Response (RFC 7807):
{
  "type": "https://api.tirotime.com/errors/validation",
  "title": "One or more validation errors occurred",
  "status": 400,
  "errors": {
    "field": ["error message"]
  },
  "traceId": "correlation-id"
}
```

### 3.3 Validation Standards
- **Input Validation**: Validate all inputs at the API boundary using FluentValidation
- **Business Rule Validation**: Enforce business rules within domain entities and aggregates
- **User-Friendly Messages**: Provide clear, actionable error messages
- **Fail Fast**: Return validation errors immediately with detailed field-level feedback

### 3.4 Authentication & Authorization
- **JWT Tokens**: Use JWT for stateless authentication
- **Role-Based Access Control (RBAC)**: Implement role-based permissions
- **Policy-Based Authorization**: Use ASP.NET Core policy-based authorization for complex scenarios
- **Secure by Default**: All endpoints require authentication unless explicitly marked as public

### 3.5 Internationalization (i18n)
- Support for multiple languages through resource files
- Date, time, and number formatting based on user locale
- All user-facing messages must be localizable

---

## 4. Performance Requirements

### 4.1 Response Time Targets
- **API Endpoints**: 95th percentile < 200ms for read operations, < 500ms for write operations
- **Database Queries**: Single query execution < 100ms
- **External Service Calls**: Timeout after 30 seconds with proper retry logic

### 4.2 Database Performance
- **Indexing Strategy**: All foreign keys and frequently queried columns must be indexed
- **Query Optimization**: Use Entity Framework Core efficiently; avoid N+1 queries
- **Connection Pooling**: Properly configure connection pooling for optimal resource usage
- **Pagination**: All list endpoints must implement pagination (default page size: 20, max: 100)
- **Projections**: Use projections (Select) to fetch only required data

### 4.3 Caching Strategy
- **Distributed Caching**: Use Redis for distributed caching in production
- **Cache Invalidation**: Implement proper cache invalidation strategies
- **Cache Keys**: Use consistent, descriptive cache key naming conventions
- **TTL Policy**: Define appropriate Time-To-Live for different data types
- **Cache Aside Pattern**: Implement cache-aside pattern for read-heavy operations

### 4.4 Asynchronous Processing
- **Async/Await**: Use async/await for all I/O-bound operations
- **Background Jobs**: Use Hangfire or similar for long-running tasks
- **Message Queues**: Implement message queues for inter-service communication and eventual consistency
- **Cancellation Tokens**: Support cancellation tokens for graceful shutdown

### 4.5 Resource Management
- **Memory Limits**: Monitor and optimize memory usage; prevent memory leaks
- **Dispose Pattern**: Properly implement IDisposable for resource cleanup
- **Connection Management**: Always dispose database connections and HTTP clients properly
- **Thread Pool**: Avoid blocking thread pool threads with synchronous I/O

### 4.6 Scalability Requirements
- **Horizontal Scaling**: Application must support horizontal scaling without session affinity
- **Stateless Design**: No in-memory session state; use distributed cache or database
- **Database Scaling**: Design schema to support read replicas and sharding if needed
- **Rate Limiting**: Implement rate limiting to prevent abuse (default: 1000 requests/hour per user)

---

## 5. Security Requirements

### 5.1 OWASP Top 10 Protection
- **Injection Prevention**: Use parameterized queries; sanitize all inputs
- **Authentication**: Implement secure password hashing (bcrypt or Argon2)
- **Sensitive Data Exposure**: Encrypt sensitive data at rest and in transit (TLS 1.2+)
- **XML External Entities (XXE)**: Disable XML external entity processing
- **Broken Access Control**: Enforce authorization checks at service layer
- **Security Misconfiguration**: Follow security best practices for all configurations
- **Cross-Site Scripting (XSS)**: Encode outputs; use Content Security Policy headers
- **Insecure Deserialization**: Validate and sanitize all deserialized data
- **Components with Known Vulnerabilities**: Keep dependencies up-to-date
- **Insufficient Logging**: Log security-relevant events with appropriate detail

### 5.2 Data Protection
- **Personal Identifiable Information (PII)**: Encrypt PII data; minimize collection
- **Secrets Management**: Use Azure Key Vault or similar for storing secrets
- **Password Policy**: Enforce strong password requirements (min 12 chars, complexity)
- **Data Retention**: Implement data retention policies and GDPR compliance

### 5.3 Security Headers
All API responses must include appropriate security headers:
- `Strict-Transport-Security`
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Content-Security-Policy`
- `X-XSS-Protection: 1; mode=block`

---

## 6. Observability & Monitoring

### 6.1 Logging Standards
- **Structured Logging**: Use Serilog with structured logging
- **Log Levels**: Use appropriate log levels (Trace, Debug, Information, Warning, Error, Critical)
- **Correlation IDs**: Include correlation IDs in all logs for request tracing
- **No Sensitive Data**: Never log passwords, tokens, or PII
- **Contextual Information**: Include relevant context (userId, operation, duration)

### 6.2 Metrics & Monitoring
- **Application Metrics**: Track response times, error rates, and throughput
- **Business Metrics**: Monitor key business KPIs through domain events
- **Health Checks**: Implement health check endpoints for all dependencies
- **Alerting**: Configure alerts for critical errors and performance degradation

### 6.3 Distributed Tracing
- Use Application Insights or OpenTelemetry for distributed tracing
- Trace requests across service boundaries
- Monitor dependencies (database, external APIs, caching)

---

## 7. Dependency Management

### 7.1 Package Standards
- **Stable Versions**: Use stable, production-ready package versions
- **Security Audits**: Regularly audit dependencies for vulnerabilities
- **Minimal Dependencies**: Avoid unnecessary dependencies; keep dependency tree lean
- **License Compliance**: Verify all dependencies use compatible licenses

### 7.2 Framework Requirements
- **.NET Version**: Use latest LTS version of .NET (currently .NET 8)
- **Entity Framework Core**: Use latest stable version for ORM
- **ASP.NET Core**: Leverage ASP.NET Core for web APIs
- **MediatR**: Use MediatR for CQRS implementation

---

## 8. Development Workflow

### 8.1 Version Control
- **Git Flow**: Follow Git Flow branching strategy
- **Commit Messages**: Use conventional commit format (feat, fix, docs, refactor, test)
- **Branch Protection**: Main/master branch requires pull requests and reviews
- **No Direct Commits**: No direct commits to main/master branch

### 8.2 Continuous Integration
- **Automated Builds**: All commits trigger automated builds
- **Test Execution**: Full test suite runs on every pull request
- **Code Quality Gates**: Failed builds or tests block merging
- **Static Analysis**: Run code analysis tools (SonarQube, Roslyn analyzers)

### 8.3 Continuous Deployment
- **Automated Deployments**: Deploy automatically after successful builds on main branch
- **Environment Strategy**: Maintain Dev → Staging → Production environments
- **Blue-Green Deployments**: Use blue-green deployment strategy for zero-downtime
- **Rollback Plan**: Always have a rollback strategy for failed deployments

---

## 9. Code Style & Conventions

### 9.1 C# Coding Standards
- Follow Microsoft C# Coding Conventions
- Use .editorconfig for consistent formatting across team
- PascalCase for public members; camelCase for private members
- Use nullable reference types to prevent null reference exceptions
- Prefer expression-bodied members for simple methods/properties

### 9.2 File Organization
- One class per file (except for nested classes)
- File names match class names
- Organize files by feature/domain context, not by technical layer
- Keep files under 300 lines; refactor when exceeding this limit

### 9.3 Naming Conventions
- **Interfaces**: Prefix with `I` (e.g., `IOrderRepository`)
- **Async Methods**: Suffix with `Async` (e.g., `GetOrderAsync`)
- **Test Classes**: Suffix with `Tests` (e.g., `OrderServiceTests`)
- **Domain Events**: Suffix with `Event` or use past tense (e.g., `OrderPlacedEvent`)

---

## 10. Enforcement & Evolution

### 10.1 Constitution Compliance
- All team members must read and understand this constitution
- Architecture reviews must verify compliance with these principles
- Violations must be discussed and resolved during code review
- Automated tools (linters, analyzers) enforce rules where possible

### 10.2 Constitution Updates
- Constitution can be updated through team consensus
- Major changes require architectural review and approval
- Updates must be versioned and communicated to all team members
- Rationale for changes must be documented

### 10.3 Exceptions
- Exceptions to these principles require explicit justification
- Document exceptions as ADRs with rationale and impact analysis
- Technical debt from exceptions must be tracked and scheduled for resolution

---

## Appendix: Recommended Tools & Libraries

### Core Framework
- .NET 8+ (LTS)
- ASP.NET Core for Web APIs
- Entity Framework Core for ORM

### Architecture & Patterns
- MediatR (CQRS implementation)
- FluentValidation (input validation)
- AutoMapper (object mapping)
- Ardalis.Specification (repository pattern)

### Testing
- xUnit (test framework)
- FluentAssertions (assertions)
- NSubstitute or Moq (mocking)
- Bogus (test data generation)
- Testcontainers (integration testing)

### Observability
- Serilog (structured logging)
- Application Insights or OpenTelemetry (monitoring)
- Polly (resilience and fault handling)

### Performance
- Redis or MemoryCache (caching)
- Hangfire (background jobs)
- MassTransit or NServiceBus (messaging)

### Security
- IdentityServer or Azure AD (authentication)
- AspNetCore.Authentication.JwtBearer (JWT handling)

---

**Version**: 1.0
**Last Updated**: 2025-11-04
**Status**: Active
