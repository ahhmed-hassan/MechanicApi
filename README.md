# MechanicShop API  
A comprehensive RESTful API following Clean Architecture for managing an automotive repair shop, enabling customers to schedule repairs and managers to oversee operations efficiently.

![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)
![MicrosoftSQLServer](https://img.shields.io/badge/Microsoft%20SQL%20Server-CC2927?style=for-the-badge&logo=microsoft%20sql%20server&logoColor=white)
![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-FFFFFF?&style=for-the-badge&logo=opentelemetry&logoColor=black)

---

## 🌟 Features  

### **Customer Management**
- Complete CRUD operations for customer profiles  
- Vehicle information management  
- Contact details and communication tracking  

### **Work Order System**
- Create and manage repair orders  
- Schedule appointments with time slots  
- Assign work to specific service bays  
- Assign and reassign labor to work orders  
- Track order status through entire lifecycle  

### **Repair Operations**
- Define and manage repair tasks  
- Parts inventory and cost tracking  
- Labor management with pricing  
- Comprehensive task estimation  

### **Invoice & Billing** *(coming soon)*
- Automated invoice generation  
- PDF export functionality  
- Payment status tracking  
- Detailed billing with tax calculations  

### **Dashboard & Analytics** *(coming soon)*
- Real-time business metrics  
- Revenue and profit tracking  
- Completion rate statistics  
- Performance analytics  

### **Authentication & Authorization**
- JWT-based secure authentication  
- Role-based access control (Manager/Labor)  
- Token refresh functionality  
- Secure claim management  

---

## 🏛️ Architecture

This project follows **Clean Architecture**, keeping business logic independent of frameworks and infrastructure concerns.

```
src/
├── MechanicDomain/           # Entities, value objects, domain events — no dependencies
├── MechanicApplication/      # Use cases, CQRS handlers, interfaces, validators
├── MechanicInfrastructure/   # EF Core, SQL Server, external services
├── MechanicApi/              # ASP.NET Core Web API — controllers, middleware, DI wiring
└── MechanicContracts/        # Shared request/response DTOs

tests/
├── MechanicApi.Domain.UnitTests/
├── MechanicShop.Application.Unittests/
├── MechanicShop.Application.subcutaneoustests/
└── MechanicShop.API.Integration/
```

Dependency flow: `Api → Application → Domain` (Infrastructure implements Application interfaces via DI).

---

## 🚀 Quick Start  

### **Prerequisites**
- .NET 9.0 SDK  
- Microsoft SQL Server  
- Git  

### **Installation**
Docker support is coming soon. Stay tuned!

---

## 🔐 Authentication

The API uses **JWT Bearer authentication**. To access protected endpoints:

1. Obtain a token from `/identity/token/generate`
2. Include the token in requests as a `Bearer` header

### **Example Login Request**
```json
{
  "email": "user@example.com",
  "password": "your_password"
}
```

### **Roles**

| Role | Capabilities |
|------|-------------|
| **Manager** | Full access — manage customers, work orders, repair tasks, and labors |
| **Labor** | View and update state of own assigned work orders |

---

## 📋 API Endpoints

### Identity Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/identity/token/generate` | Obtain JWT token |
| POST | `/identity/token/refresh-token` | Refresh access token |
| GET | `/identity/current-user/claims` | Get current user info |

### Customers

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/customers` | List all customers |
| POST | `/api/v1/customers` | Create new customer (Manager only) |
| GET | `/api/v1/customers/{id}` | Get customer details |
| PUT | `/api/v1/customers/{id}` | Update customer (Manager only) |
| DELETE | `/api/v1/customers/{id}` | Delete customer (Manager only) |

### Work Orders

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/workorders` | Paginated work orders with filtering and sorting |
| POST | `/api/v1/workorders` | Create new work order (Manager only) |
| GET | `/api/v1/workorders/{id}` | Get work order details |
| PUT | `/api/v1/workorders/{id}/state` | Update work order state (Manager or assigned Labor) |
| PUT | `/api/v1/workorders/{id}/labor` | Assign labor to work order (Manager only) |
| PUT | `/api/v1/workorders/{id}/repair-tasks` | Update repair tasks on a work order (Manager only) |
| POST | `/api/v1/workorders/{id}/relocate` | Relocate work order to new time/spot (Manager only) |
| DELETE | `/api/v1/workorders/{id}` | Delete work order (Manager only) |

### Repair Tasks

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/repair-tasks` | List all repair tasks |
| POST | `/api/v1/repair-tasks` | Create new repair task (Manager only) |
| GET | `/api/v1/repair-tasks/{id}` | Get repair task details |
| PUT | `/api/v1/repair-tasks/{id}` | Update repair task (Manager only) |
| DELETE | `/api/v1/repair-tasks/{id}` | Delete repair task (Manager only) |

### Labors

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/labors` | List all labor employees |

### Invoices *(coming soon)*

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/invoices/workorders/{id}` | Generate invoice for a work order |
| GET | `/api/v1/invoices/{id}` | Get invoice details |
| GET | `/api/v1/invoices/{id}/pdf` | Download PDF invoice |
| PUT | `/api/v1/invoices/{id}/payments` | Mark invoice as paid |

### Dashboard *(coming soon)*

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/dashboard/stats` | Get business statistics |
| GET | `/api/v1/workorders/schedule/{date}` | Get daily schedule |

---

## 🏗️ Data Models

| Entity | Description |
|--------|-------------|
| **Customer** | Shop clients with contact information |
| **Vehicle** | Customer vehicles (make, model, license) — cannot exist without a customer |
| **WorkOrder** | Repair orders with scheduling and status tracking |
| **RepairTask** | Individual repair operations with parts and labor costs |
| **Employee** | Shop employees with a role of Manager or Labor |
