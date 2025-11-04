# TiroTime Specifications

This directory contains comprehensive specifications for the TiroTime time tracking application.

## Overview

TiroTime is a modern web-based time tracking application designed for freelancers, consultants, and agencies. Built using .NET 8, Domain-Driven Design (DDD), and Clean Architecture principles, it enables accurate time tracking, project management, and professional report generation with Excel and PDF export capabilities.

## Document Structure

### Core Documents

1. **[001 - TiroTime Overview](./001-tirotime-overview.md)**
   - Executive summary and vision
   - High-level requirements
   - Success metrics and project phases
   - **Start here** for project overview

2. **[Constitution](../constitution.md)**
   - Code quality principles
   - Testing standards
   - User experience consistency
   - Performance requirements
   - Security and compliance guidelines

### Feature Specifications

3. **[002 - User Management](./features/002-user-management.md)**
   - Authentication (registration, login, password reset)
   - User roles (Admin, Manager, Employee)
   - Profile management
   - Security and access control
   - Role-based permissions

4. **[003 - Time Tracking](./features/003-time-tracking.md)**
   - Timer functionality (start/stop)
   - Manual time entry methods
   - Duration-based entry
   - Timesheet views
   - Approval workflows
   - Tags and categorization
   - Idle detection

5. **[004 - Client & Project Management](./features/004-client-project-management.md)**
   - Client CRUD operations
   - Project management
   - Team assignments
   - Hourly rates and budgets
   - Project lifecycle management

6. **[005 - Reporting & Export](./features/005-reporting-export.md)**
   - Time entry reports with filtering
   - Excel export with German formatting
   - PDF generation for invoicing
   - Dashboard and analytics
   - Export templates and customization
   - Invoice generation (Phase 3)

### Implementation Guides

7. **[006 - User Stories & Use Cases](./user-stories/006-user-stories.md)**
   - Detailed user personas
   - 28 user stories across all features
   - Complete use case workflows
   - Acceptance criteria
   - Priority and phase assignments

8. **[007 - Domain Model](./domain/007-domain-model.md)**
   - Bounded contexts (4 contexts)
   - Aggregates, entities, and value objects
   - Domain events and services
   - Repository interfaces
   - Ubiquitous language dictionary
   - Complete DDD tactical patterns

9. **[008 - Technical Architecture](./architecture/008-technical-architecture.md)**
   - Clean Architecture layers
   - Technology stack (.NET 8, EF Core, PostgreSQL/SQL Server)
   - Project structure and organization
   - CQRS implementation with MediatR
   - Security architecture (JWT, RBAC)
   - Caching strategy (Redis)
   - API design and endpoints
   - Database schema
   - Performance optimization
   - Deployment architecture

## Quick Navigation by Role

### For Product Managers
- Start with [001 - Overview](./001-tirotime-overview.md)
- Review [006 - User Stories](./user-stories/006-user-stories.md)
- Check feature specifications (002-005)

### For Developers
- Read [Constitution](../constitution.md) for coding standards
- Study [007 - Domain Model](./domain/007-domain-model.md)
- Review [008 - Technical Architecture](./architecture/008-technical-architecture.md)
- Reference feature specs for implementation details

### For Architects
- Review [008 - Technical Architecture](./architecture/008-technical-architecture.md)
- Study [007 - Domain Model](./domain/007-domain-model.md)
- Check [Constitution](../constitution.md) for architectural principles

### For QA Engineers
- Study [006 - User Stories](./user-stories/006-user-stories.md)
- Review acceptance criteria in feature specs
- Check testing requirements in each feature spec
- Reference [Constitution](../constitution.md) for testing standards

### For UX Designers
- Review [006 - User Stories](./user-stories/006-user-stories.md) for personas
- Check feature specifications for UI requirements
- See [005 - Reporting](./features/005-reporting-export.md) for export formats

## Key Features Summary

### Core Functionality (MVP - Phase 1)
- ✅ User authentication and authorization
- ✅ Client and project management
- ✅ Start/stop timer for real-time tracking
- ✅ Manual time entry with start/end times
- ✅ Duration-based time entry
- ✅ Time entry editing and deletion
- ✅ Basic Excel export
- ✅ German localization support

### Enhanced Features (Phase 2)
- 📋 Calendar view time entry
- 📋 Weekly timesheet grid
- 📋 PDF export with professional formatting
- 📋 Dashboard with analytics
- 📋 Project budget tracking
- 📋 Time entry approval workflow
- 📋 Tags and categorization
- 📋 Export templates

### Advanced Features (Phase 3)
- 🔮 Idle detection
- 🔮 Invoice generation
- 🔮 Report scheduling
- 🔮 Advanced team management
- 🔮 Bulk operations
- 🔮 Saved reports

### Enterprise Features (Phase 4+)
- 🚀 Multi-tenancy
- 🚀 Calendar integrations (Google, Outlook)
- 🚀 Mobile native apps
- 🚀 AI-powered time suggestions
- 🚀 Advanced analytics and forecasting

## Technical Highlights

- **Architecture**: Clean Architecture with DDD
- **Framework**: .NET 8 (LTS)
- **Database**: PostgreSQL or SQL Server
- **Caching**: Redis for distributed caching
- **API**: RESTful with CQRS pattern
- **Real-time**: SignalR for timer synchronization
- **Background Jobs**: Hangfire for async processing
- **Testing**: xUnit, FluentAssertions, NSubstitute
- **Security**: JWT authentication, bcrypt password hashing
- **Exports**: EPPlus/ClosedXML (Excel), QuestPDF (PDF)

## Development Approach

### Domain-Driven Design
- 4 bounded contexts (Identity, Client & Project, Time Tracking, Reporting)
- Rich domain models with behavior
- Domain events for decoupling
- Ubiquitous language throughout

### CQRS Pattern
- Commands for write operations
- Queries for read operations
- MediatR for request handling
- Optimized read models

### Clean Architecture
- Domain layer (no dependencies)
- Application layer (use cases)
- Infrastructure layer (technical details)
- Presentation layer (API)

## Compliance & Standards

### Security
- OWASP Top 10 protection
- Data encryption (at rest and in transit)
- GDPR compliance
- Secure password policies
- Rate limiting and abuse prevention

### German Requirements
- German language support
- German date/number formatting (DD.MM.YYYY, 1.234,56 €)
- Invoice format compliance (Rechnung)
- VAT/MwSt handling
- Data retention compliance

### Code Quality
- 80%+ test coverage
- Automated code analysis
- Code review process
- Continuous integration
- Documentation standards

## Getting Started

1. **Read the Overview** - [001-tirotime-overview.md](./001-tirotime-overview.md)
2. **Understand the Constitution** - [constitution.md](../constitution.md)
3. **Study the Domain Model** - [007-domain-model.md](./domain/007-domain-model.md)
4. **Review the Architecture** - [008-technical-architecture.md](./architecture/008-technical-architecture.md)
5. **Pick a Feature** - Start with [002-user-management.md](./features/002-user-management.md)
6. **Begin Implementation** - Follow the technical architecture guidelines

## Document Status

| Document | Status | Last Updated | Reviewers Needed |
|----------|--------|--------------|------------------|
| 001 - Overview | Draft | 2025-11-04 | Product, Dev Team |
| 002 - User Management | Draft | 2025-11-04 | Dev, Security Team |
| 003 - Time Tracking | Draft | 2025-11-04 | Dev, UX Team |
| 004 - Client & Project | Draft | 2025-11-04 | Dev, Product Team |
| 005 - Reporting | Draft | 2025-11-04 | Dev, Finance Team |
| 006 - User Stories | Draft | 2025-11-04 | Product, UX Team |
| 007 - Domain Model | Draft | 2025-11-04 | Architecture Team |
| 008 - Technical Architecture | Draft | 2025-11-04 | Architecture, DevOps |
| Constitution | Active | 2025-11-04 | All Teams |

## Version History

- **v1.0** (2025-11-04) - Initial specifications created
  - Complete feature specifications
  - Domain model and bounded contexts
  - Technical architecture
  - User stories and use cases

## Contributing

When updating specifications:
1. Maintain document structure and formatting
2. Update version history
3. Mark status as "Draft" until reviewed
4. Add reviewers needed
5. Link related documents
6. Update this README if adding new documents

## Questions or Feedback?

For questions about these specifications, please contact:
- **Product Team**: Product requirements and user stories
- **Architecture Team**: Technical architecture and domain model
- **Development Team**: Implementation details and feasibility

---

**Generated with Claude Code** 🤖
**Project**: TiroTime - Modern Time Tracking Application
**Created**: November 4, 2025
