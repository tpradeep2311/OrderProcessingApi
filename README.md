# 🛒 Order Processing & Inventory Management System

A lightweight e-commerce backend built using **ASP.NET Core Web API** that supports product management, order processing, inventory tracking, and asynchronous order fulfillment.

---

## 📌 Features

- 🔧 **Product Management** (CRUD operations)
- 📦 **Inventory Control** (live stock tracking & reservation)
- 🧾 **Order Processing**
  - Place, cancel, and fulfill orders
  - Real-time inventory checks
- ⏳ **Asynchronous Fulfillment**
  - Background processing of orders
- 📣 **Simulated Notifications**
  - Logs fulfillment messages
- 🛡️ **Concurrency-Safe**
  - Handles simultaneous order requests with thread safety

---

## 🏗️ Architecture Overview

Client → API Controllers → Services → Repositories → EF Core (In-Memory / SQL)
                             ↓
                    Background Worker
                             ↓
                   Notification Service

- **API Layer**: Receives and routes HTTP requests
- **Service Layer**: Business logic for orders/products
- **Repository Layer**: Interfaces for data access (testable)
- **Background Worker**: Async processing of orders
- **Notification**: Logs a "confirmation" to simulate email

---

## 🚀 Tech Stack

- ASP.NET Core Web API (.NET 6+)
- Entity Framework Core
- xUnit & Moq for testing
- Hosted Services for async jobs
- Serilog (optional logging)
- Dependency Injection throughout

---

## 📬 API Endpoints

### 🔹 Products

| Method | Endpoint               | Description           |
|--------|------------------------|-----------------------|
| GET    | /api/products          | Get all products      |
| GET    | /api/products/{id}     | Get product by ID     |
| POST   | /api/products          | Add new product       |
| PUT    | /api/products/{id}     | Update product        |
| DELETE | /api/products/{id}     | Delete product        |

### 🔹 Orders

| Method | Endpoint               | Description           |
|--------|------------------------|-----------------------|
| POST   | /api/orders            | Place an order        |
| DELETE | /api/orders/{id}       | Cancel an order       |

---

## ⚙️ Concurrency & Fulfillment

- Inventory is **reserved at order time**.
- Orders are marked **"Pending Fulfillment"**.
- A **background worker** processes orders after a delay, marking them as **"Fulfilled"** and logging a simulated notification.
- Inventory updates are made **thread-safe** through transactions and/or optimistic concurrency.

---

## 🧪 Unit Testing

- Written using **xUnit**
- **Moq** used for dependency mocking
- Covers:
  - Order placement and validation
  - Cancellation logic
  - Fulfillment logic
  - Notification triggering
  - Concurrent order simulation

## 🧱 Project Structure
├── Controllers
├── Services
│   ├── ProductService.cs
│   └── OrderService.cs
├── Repositories
│   ├── Interfaces
│   └── Implementations
├── Models
├── Background (OrderFulfillmentWorker.cs)
├── Notifications (Simulated Notification Service)
├── Tests (xUnit + Moq)
