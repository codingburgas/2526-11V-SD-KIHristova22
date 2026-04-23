[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/U2LjJyKI)
# 📚 BookStore Inventory & Orders System
ASP.NET Core MVC web application for managing an online bookstore, including book inventory, categories, and customer orders.
---
## 🎯 Project Overview
This project was developed as part of a school assignment (XI grade, 2025/2026).
The system supports two types of users:
- 👤 **User (Customer)** – can browse books and create orders
- 🛠️ **Admin** – manages books, categories, and orders.
  <br>
  The application follows the **ASP.NET Core MVC architecture** and uses **Entity Framework Core (Code-First)** for database management.
---
## ⚙️ Technologies Used
- ASP.NET Core MVC (.NET 10)
- Entity Framework Core (Code-First)
- SQLite Database
- ASP.NET Core Identity (Authentication & Roles)
- Bootstrap (Responsive UI)
- LINQ (Queries & Reports)
---
## 🗂️ Features
### 📖 Books
- View all books
- Filter by category
- View book details
- Admin can add books
### 📂 Categories
- View categories
- Admin can manage categories
### 🛒 Orders
- Create orders
- View order details
- Admin can see all orders
### 📊 Reports
- Top 5 most sold books (LINQ query)
---
## 🔐 Authentication & Authorization
The system uses **ASP.NET Core Identity** with role-based access:
### 👤 User
- Browse books
- View details
- Create orders
### 🛠️ Admin
- Full CRUD for books and categories
- View all orders
- Access reports
---
## 👥 Default Accounts
### Admin
- Email: admin@bookstore.com
- Password: Admin123!
### User
- Email: user@bookstore.com
- Password: User123!
---
## 🧱 Architecture
The project follows standard MVC structure:
- **Models** – database entities (Book, Category, Order, OrderItem)
- **Views** – Razor UI pages
- **Controllers** – handle requests and business logic
  Additional:
- **ViewModels** used for data transfer
- **Dependency Injection** used for services
- **EF Core Migrations** for database
---
## 🗄️ Database
- SQLite database (`app.db`)
- Created using Code-First approach
- Relationships:
- Category → Books (1:M)
- User → Orders (1:M)
- Order ↔ Books (M:M via OrderItems)
---
## 🚀 How to Run
1. Clone the repository
2. Open the solution in Visual Studio
3. Run the project
4. Database will be created automatically
5. Default accounts will be seeded
---
## 📸 Screenshots

---
## 📌 Project Structure
- Controllers/
- Models/
- Views/
- ViewModels/
- Data/
- Migrations/
- wwwroot/
---
## 🧠 Key Concepts Implemented
- MVC Architecture
- EF Core Code-First
- LINQ Queries
- Role-based Authorization
- Dependency Injection
- Data Validation