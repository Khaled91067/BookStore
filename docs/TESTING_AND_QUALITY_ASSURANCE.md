# BookStore - Testing & Quality Assurance Specification

## Overview

Testing for **BookStore** relies on automated unit tests implemented in `BookStore.Tests`. Unit tests validate business service logic, catalog search boundaries, order processing rules, and user management services.

---

## Testing Topology & Frameworks

* **Target Framework**: .NET 10.0
* **Testing Framework**: **xUnit**
* **Mocking Framework**: **Moq**
* **Database Provider for Testing**: Entity Framework Core In-Memory Database provider.

```text
BookStore.Tests/
└── Services/       # Unit test suites covering application business services
```

---

## Test Suite Scope

### 1. Catalog & Search Service Tests
* **Keyword Search**: Verifies that title searches filter products correctly.
* **Category & Author Filtering**: Verifies that multi-criteria filters return matching products.
* **Pagination**: Validates page index and page size calculations.

### 2. Order Processing & Inventory Control Tests
* **Inventory Allocation**: Verifies that order creation decreases product stock levels by ordered quantities.
* **Stock Exhaustion Rules**: Verifies that ordering items beyond available stock returns failure results.
* **Order Financial Calculation**: Validates total amount calculations (`Sum(Price * Quantity)`).
* **State Lifecycle Transitions**: Validates order status values.

### 3. Identity & User Management Tests
* **User Querying**: Validates user retrieval services.
* **Role Allocation**: Verifies role assignment logic using mocked identity managers.

---

## Executing Automated Tests

### Command Line Interface
Execute all unit tests from the solution root:

```bash
dotnet test --configuration Release --verbosity normal
```

### Selective Test Execution
```bash
# Execute specific service test suites by filter
dotnet test --filter "FullyQualifiedName~Services"
```

---

## Developer Testing Guidelines

1. **AAA Pattern**: Unit tests follow the **Arrange-Act-Assert** pattern.
2. **Test Isolation**: Each unit test executes independently without shared static state.
3. **Database Instance Isolation**: In-memory database context instances generate unique database names per test run to prevent cross-test data pollution.
4. **External Boundary Mocking**: External dependencies (such as external payment gateway APIs) are mocked using `Moq`.
