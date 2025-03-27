# 🏢 Employee Leave Management System
## 📖 Overview
The Employee Leave Management System is a web application designed to manage employee leave requests. It allows employees to request vacation or sick leave, and HR managers to approve or reject these requests. The system also provides dashboards for both employees and HR managers to view leave balances, recent requests, and other relevant information.

## ✨ Features
* 👤 Employee Features:
    * 📅 Request vacation or sick leave.
    * 📊 View remaining leave balances (annual, bonus, and sick leave).
    * 📋 View recent leave requests and their statuses (approved, pending, rejected).
* 👔 HR Features:
    * ✅ Approve or reject leave requests.
    * 👥 Manage employee profiles (add, update, delete employees).
    * 📊 View leave reports and export them to Excel.
    * 🔍 Monitor employees with low leave balances.
* 🔐 Authentication and Authorization:
    * 👤 Employees and HR managers have separate roles and access levels.
    * 🚪 Employees can only access their own dashboard and leave requests.
    * 🗝️ HR managers can access all employee data and manage leave requests.
  
## 🛠️ Technologies Used
* Backend: ASP.NET Core (MVC)
* Frontend: Razor Views, HTML, CSS, Bootstrap
* Database: SQLite (for development), Entity Framework Core (ORM)
* Authentication: ASP.NET Core Identity
* Testing: xUnit, Moq
  
## 🚀 Getting Started
📋 Prerequisites
* .NET 7 SDK
* Visual Studio or Visual Studio Code
* SQLite (for development)
  
## 🛠️ Installation
1. Clone the Repository: bash Copy  git clone https://github.com/IvaNaskk/LeaveManagementSystem
2. cd LeaveManagementSystem
   
## 🔑 Default Accounts
* HR Manager:
    * 📧 Email: admin@admin.com
    * 🔑 Password: NewSecurePass@123
* Employee:
    * 👤 Employees can register themselves using the Register page.

## 🧪 Running Tests
To run the unit tests, use the following command:
bash
Copy
dotnet test

## 📂 Project Structure
* Controllers:
    * EmployeeController: Handles employee-related actions (e.g., requesting leave, viewing dashboard).
    * HRController: Handles HR-related actions (e.g., managing employees, approving leave requests).
    * LeaveRequestController: Handles leave request approvals and rejections.
    * AccountController: Handles authentication (login, registration, logout).
    * HomeController: Handles the home page and error handling.
* Models:
    * Employee: Represents an employee with properties like name, email, and leave balances.
    * VacationRequest: Represents a vacation leave request.
    * SickLeaveRequest: Represents a sick leave request.
    * ErrorViewModel: Represents the model for the error page.
* ViewModels:
    * EmployeeDashboardViewModel: ViewModel for the employee dashboard.
    * HRDashboardViewModel: ViewModel for the HR dashboard.
    * VacationRequestViewModel: ViewModel for vacation leave requests.
    * SickLeaveRequestViewModel: ViewModel for sick leave requests.
* Views:
    * Razor views for all pages (e.g., Dashboard.cshtml, RequestVacation.cshtml, ManageEmployees.cshtml).
* Tests:
    * Unit tests for controllers and models using xUnit and Moq.

## 📸 Screenshots
Here are some screenshots of the application:

Employee Dashboard

<img width="658" alt="Screenshot 2025-03-27 at 01 39 14" src="https://github.com/user-attachments/assets/15e3659a-99b5-4db5-bd12-667a5d33fe80" />

HR Dashboard

￼￼<img width="658" alt="Screenshot 2025-03-27 at 01 38 07" src="https://github.com/user-attachments/assets/9cdd965b-b504-4f37-83dc-d0cc0a56695c" />

Leave Request Form for Annual Leave

<img width="658" alt="Screenshot 2025-03-27 at 01 40 21" src="https://github.com/user-attachments/assets/2c44be06-fe44-43d4-94ef-955ea3387275" />

Leave Request Form for Sick Leave

<img width="658" alt="Screenshot 2025-03-27 at 01 41 19" src="https://github.com/user-attachments/assets/641033cb-87eb-417f-9a73-813b2ba809db" />

￼
