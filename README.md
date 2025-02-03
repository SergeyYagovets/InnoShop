# Overview

The "InnoShop" project is a system consisting of two microservices designed for managing users and their products. 
These microservices are developed using ASP.NET Core and interact through an API.

### Running the Project

To run the project, follow these steps:
1.	Install the Docker app on your device.
2.	Open a terminal or command prompt and navigate to the project directory.
3.	To start the containers, run the command 
```javascript  
docker-compose up
```

### Features
- User registration and login functionality
- Password hashing and verification 
- Email notifications for registration and password reset
- JWT-based authentication for secure API access
- Integration with Product Service 

### Services
- User Service: responsible for user registration, login, and profile management
- Authentication Service: handles JWT-based authentication and authorization
- Email Service: sends email notifications for registration and password reset
- Product Service: provides endpoints for product management and ordering

### Endpoints

#### Access Levels
- Public: Anyone can access the endpoint
- User: The user who owns the resource can access the endpoint
- Admin: Only administrators can access the endpoint

#### Auth 
1. POST /auth/register
   - Summary: Register a new user. Note: The first user registered becomes an admin
   - Access: Public
2. POST /auth/login
   - Summary: Login to the application and receive a JWT token
   - Access: Public
3. POST /auth/forgotpassword 
   - Summary: Sends an email to the user with a link to reset their password
   - Access: Public
4. POST /auth/resetpassword
   - Summary: Resets a user's password using a reset token that was received via email
   - Access: Public

#### User

1. GET /User/get-users
   - Summary: Get all users
   - Access: Admin
2. GET /User/get-user{id}
   - Summary: Get a user by ID
   - Access: Admin
3. POST /User/create-admin
   - Summary: Creates a new admin. Note: An already created admin can create another admin.
   - Access: Admin
4. PUT /User/change-name
   - Summary: Change a user's name
   - Access: User, Admin
5. DELETE /User/delete-user{id}
   - Summary: Delete a user by ID
   - Access: Admin
6. PATH /User/{userId}/activate
   - Summary: Activates or deactivates a user by ID. When a user is deactivated, all of their products are marked as isDeleted. The isDeleted status is reversed when the user is reactivated (Soft delete)
   - Access: Admin

#### Products

1. POST /products/create-product
   - Summary: Creates a new product
   - Access: User, Admin
2. GET /products/public/get-available-products
   - Summary: Get a list of all available products based on filter parameters. Note: This method returns only products where isActive = true.
   - Access: Public
3. GET /products/public/get-available-product{id}
   - Summary: Get a product by ID. Note: This method returns only products where isActive = true.
   - Access: Public
4. GET /products/get-current-user-products
   - Summary: Get a list of all products owned by the current user based on filter parameters. Note: An admin can see all products.
   - Access: User, Admin
5. GET /products/get-current-user-product{id}
   - Summary: Get the product created by the current user by ID. Note: An admin can get any product by ID.
   - Access: User, Admin
6. PUT /products/update-product{id}
   - Summary: Updates a product by ID that was created by the current user. Note: An admin can update any product by ID.
   - Access: User, Admin
7. DELETE /products/delete-product{id}
   - Summary: Deletes a product by ID that was created by the current user. Note: An admin can delete any product by ID.
   - Access: User, Admin

  
