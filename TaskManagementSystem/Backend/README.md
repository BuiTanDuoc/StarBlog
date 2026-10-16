# Task Management System - Backend API

A powerful, enterprise-grade REST API built with ASP.NET Core 8 for collaborative project and task management. 

**Live at:** https://localhost:5001 | **API Docs:** /swagger

## 🌟 Features

### Project Management
- Create, edit, and delete projects with full CRUD operations
- Project member management with role-based access (Owner/Member/Viewer)
- Real-time member and task counting
- Soft-delete support for data recovery

### Task Management  
- Complete task lifecycle management (Backlog → In Progress → Review → Done)
- Priority-based task organization (Low/Medium/High/Critical)
- Task assignment to team members with deadline management
- Advanced task search and filtering
- Task status updates with real-time feedback

### Team Collaboration
- User search and membership integration
- Add/remove team members with flexible role assignment
- Re-add previously removed members seamlessly
- Role-based authorization checks

### Comments & Discussion
- Add comments to tasks for team discussion
- View all comments with user attribution and timestamps
- Delete own comments with permission checks
- Real-time comment updates

### Security & Authentication
- JWT Bearer Token authentication
- HTTP-only cookie storage for token security
- Role-based access control (RBAC)
- Protected endpoints with [Authorize] attributes

## 🏗️ Architecture

### Tech Stack
| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 9.0.9 |
| Database | SQL Server | 2019+ |
| Authentication | JWT Bearer Tokens | Built-in |
| Logging | Serilog | 9.0.0 |
| API Docs | Swagger/OpenAPI | 6.6.2 |
| Mapping | AutoMapper | 15.0.1 |

### Project Structure
