# MechanicShop API  
A comprehensive RESTful API following Clean Architecture for managing an automotive repair shop, enabling customers to schedule repairs and managers to oversee operations efficiently.

![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)
![MicrosoftSQLServer](https://img.shields.io/badge/Microsoft%20SQL%20Server-CC2927?style=for-the-badge&logo=microsoft%20sql%20server&logoColor=white)
![Swagger](https://img.shields.io/badge/-Swagger-%23Clojure?style=for-the-badge&logo=swagger&logoColor=white)
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
- Track order status through entire lifecycle  

### **Repair Operations**
- Define and manage repair tasks  
- Parts inventory and cost tracking  
- Labor management with pricing  
- Comprehensive task estimation  

### **Invoice & Billing**
- Automated invoice generation  
- PDF export functionality  
- Payment status tracking  
- Detailed billing with tax calculations  

### **Dashboard & Analytics**
- Real-time business metrics  
- Revenue and profit tracking  
- Completion rate statistics  
- Performance analytics  

### **Authentication & Authorization**
- JWT-based secure authentication  
- Role-based access control (Managers/Customers)  
- Token refresh functionality  
- Secure claim management  

---

## 🚀 Quick Start  

### **Prerequisites**
- .NET 7.0 SDK  
- PostgreSQL 15+  
- Git  

### **Installation**
Docker is about to add. Stay tuned! <br>
**Clone the repository**


## 🔐 Authentication

The API uses **JWT Bearer authentication**. To access protected endpoints:

1. Obtain a token from `/identity/token/generate`
2. Include the token in requests:
### **Example Login Request**
```json
{
  "email": "user@example.com",
  "password": "your_password"
}
```

## 📋 API Endpoints
### Identity Management

- POST /identity/token/generate - Obtain JWT token

- POST /identity/token/refresh-token - Refresh access token

- GET /identity/current-user/claims - Get current user info

### Customers

- GET /api/v1/customers - List all customers

- POST /api/v1/customers - Create new customer

- GET /api/v1/customers/{id} - Get customer details

- PUT /api/v1/customers/{id} - Update customer

- DELETE /api/v1/customers/{id} - Delete customer

### Work Orders

- GET /api/v1/workorders - Paginated work orders with filtering

- POST /api/v1/workorders - Create new work order

- GET /api/v1/workorders/{id} - Get work order details

- PUT /api/v1/workorders/{id}/state - Update work order state

- DELETE /api/v1/workorders/{id} - Delete work order

### Invoices

- POST /api/v1/invoices/workorders/{id} - Generate invoice

- GET /api/v1/invoices/{id} - Get invoice details

- GET /api/v1/invoices/{id}/pdf - Download PDF invoice

- PUT /api/v1/invoices/{id}/payments - Mark as paid

### Repair Tasks

- GET /api/v1/repair-tasks - List all repair tasks

- POST /api/v1/repair-tasks - Create new repair task

- PUT /api/v1/repair-tasks/{id} - Update repair task

- DELETE /api/v1/repair-tasks/{id} - Delete repair task

### Dashboard

- GET /api/v1/dashboard/stats - Get business statistics

- GET /api/v1/workorders/schedule/{date} - Get daily schedule

## 🏗️ Data Models

### Key Entities:

- Customer: Represents shop clients with contact information

- Vehicle: Customer vehicles with make, model, and license details. It cannot exist without a customer

- WorkOrder: Repair orders with scheduling and status tracking

- RepairTask: Individual repair operations with parts and labor

- Invoice: Billing documents with payment tracking

