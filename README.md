\# Enterprise Employee Management System



A secure and scalable Employee Management REST API built with ASP.NET Core Web API, Entity Framework Core, SQL Server, JWT Authentication, Refresh Tokens, Role-Based Authorization, and Clean Architecture principles.



\## 🚀 Project Overview



Enterprise Employee Management is a backend API designed to manage employees and their organizational data.



The system provides:



\- Employee management

\- Department management

\- Role management

\- Attendance management

\- Leave request management

\- User registration and login

\- JWT authentication

\- Refresh token authentication

\- Logout and refresh-token revocation

\- Role-based authorization

\- Entity Framework Core

\- SQL Server database

\- Swagger/OpenAPI documentation

\- Clean Architecture structure

\- Repository and Unit of Work patterns



\---



\## 🛠️ Technologies Used



\- C#

\- .NET 10

\- ASP.NET Core Web API

\- Entity Framework Core

\- SQL Server

\- JWT Authentication

\- Refresh Tokens

\- Swagger / OpenAPI

\- REST API

\- Clean Architecture

\- Repository Pattern

\- Unit of Work Pattern

\- Dependency Injection

\- LINQ

\- BCrypt Password Hashing



\---



\## 🏗️ Architecture



The project follows a Clean Architecture approach:



```text

EnterpriseEmployeeManagement

│

├── EnterpriseEmployeeManagement.Domain

│   ├── Common

│   └── Entities

│

├── EnterpriseEmployeeManagement.Application

│   ├── DTOs

│   └── Interfaces

│

├── EnterpriseEmployeeManagement.Infrastructure

│   ├── Persistence

│   ├── Repositories

│   ├── Services

│   ├── Authentication

│   └── Migrations

│

└── EnterpriseEmployeeManagement.WebApi

&#x20;   ├── Controllers

&#x20;   ├── Program.cs

&#x20;   └── appsettings.json

