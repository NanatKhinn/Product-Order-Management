# Product Order Management System (.NET 10 API + ASP.NET Core MVC & React)

This is a full-stack web application featuring a .NET 10 RESTful API backend with JWT authentication (Role-based: Admin & User) and an interactive ASP.NET Core MVC frontend utilizing React via CDN and Axios.

---

## Features

- **Authentication & Authorization**: JWT-based login/register with Role-Based Access Control (`Admin` and `User`).
- **Admin Dashboard**: Product Management (CRUD operations) and viewing all customer orders.
- **Customer Portal**: Product browsing, shopping cart management, stock validation, order placement, and personal order history tracking.
- **Dynamic UI**: Navbar dynamically adapts based on the decoded JWT role stored in `localStorage`.

---

## Tech Stack

- **Backend**: .NET 10, ASP.NET Core Web API, Entity Framework Core, MS SQL Server, ASP.NET Identity (JWT Bearer).
- **Frontend**: ASP.NET Core MVC Razor Views, React 18 (via CDN), Babel, Axios, Bootstrap 5.

---

## Prerequisites

Make sure you have the following installed on your machine:
- .Net 10 SDK
- Visual Studio 2026
- SQL Server Management Studio  

---

## Getting Started & Installation

### 1. Clone the Repository
### 2 . Navigate to API project folder and open appsettings.json. Update your database connection string and JWT settings.
### 3. Apply Database Migrations
dotnet ef database update
### 4. In MVC project's appsettings.json, ensure the API base URL matches the running backend port.
### 5. You need to run both the Backend API and the MVC Frontend together.
### 6. You can test the work flow for both user and admin.
