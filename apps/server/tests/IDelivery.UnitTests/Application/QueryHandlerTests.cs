using FluentAssertions;
using IDelivery.Application.Abstractions.Authentication;
using IDelivery.Application.Abstractions.Persistence;
using IDelivery.Application.Queries.Catalog;
using IDelivery.Application.Queries.Carts;
using IDelivery.Application.Queries.Customers;
using IDelivery.Application.Queries.Delivery;
using IDelivery.Application.Queries.Orders;
using IDelivery.Application.Queries.Payments;
using IDelivery.Application.Queries.Tenants;
using IDelivery.Domain.Catalog.Entities;
using IDelivery.Domain.Carts.Entities;
using IDelivery.Domain.Common.ValueObjects;
using IDelivery.Domain.Customers.Entities;
using IDelivery.Domain.Delivery.Entities;
using IDelivery.Domain.Delivery.Enums;
using IDelivery.Domain.Orders.Entities;
using IDelivery.Domain.Orders.Enums;
using IDelivery.Domain.Payments.Entities;
using IDelivery.Domain.Payments.Enums;
using IDelivery.Domain.Tenants.Entities;
using IDelivery.Domain.Tenants.Enums;
using Moq;
using Xunit;

namespace IDelivery.UnitTests.Application;

public class GetOrderQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetOrderQueryHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnOrder()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var money = Money.Create(10.00m, "BRL").Value;
        var orderItem = OrderItem.Create(orderId, productId, "Coca-Cola", money, 2).Value;
        var address = Address.Create("Rua A", "123", null, "Centro", "São Paulo", "SP", "01001-000", null).Value;
        var orderResult = Order.Create(_tenantId, customerId, [orderItem], Money.Create(5.00m, "BRL").Value, address, 2.5m);
        var order = orderResult.Value;

        _mockOrderRepository.Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var query = new GetOrderQuery(orderId);
        var handler = new GetOrderQueryHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
        result.Value.TenantId.Should().Be(_tenantId);
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var query = new GetOrderQuery(Guid.NewGuid());
        var handler = new GetOrderQueryHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldFail()
    {
        _mockOrderRepository.Setup(x => x.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var query = new GetOrderQuery(Guid.NewGuid());
        var handler = new GetOrderQueryHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }

    [Fact]
    public async Task Handle_WithDifferentTenant_ShouldFail()
    {
        var otherTenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var money = Money.Create(10.00m, "BRL").Value;
        var orderItem = OrderItem.Create(orderId, productId, "Coca-Cola", money, 2).Value;
        var address = Address.Create("Rua A", "123", null, "Centro", "São Paulo", "SP", "01001-000", null).Value;
        var orderResult = Order.Create(otherTenantId, customerId, [orderItem], Money.Create(5.00m, "BRL").Value, address, null);
        var order = orderResult.Value;

        _mockOrderRepository.Setup(x => x.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var query = new GetOrderQuery(orderId);
        var handler = new GetOrderQueryHandler(
            _mockOrderRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.AccessDenied");
    }
}

public class GetOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetOrdersQueryHandlerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedOrders()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var money = Money.Create(10.00m, "BRL").Value;
        var orderItem = OrderItem.Create(orderId, productId, "Coca-Cola", money, 2).Value;
        var address = Address.Create("Rua A", "123", null, "Centro", "São Paulo", "SP", "01001-000", null).Value;
        var orderResult = Order.Create(_tenantId, customerId, [orderItem], Money.Create(5.00m, "BRL").Value, address, null);
        var order = orderResult.Value;

        _mockOrderRepository.Setup(x => x.GetByTenantIdAsync(_tenantId, It.IsAny<OrderState?>(), 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { order });
        _mockOrderRepository.Setup(x => x.CountByTenantIdAsync(_tenantId, It.IsAny<OrderState?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var query = new GetOrdersQuery(null, 1, 20);
        var handler = new GetOrdersQueryHandler(_mockOrderRepository.Object, _mockTenantContext.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var query = new GetOrdersQuery(null, 1, 20);
        var handler = new GetOrdersQueryHandler(_mockOrderRepository.Object, _mockTenantContext.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.TenantRequired");
    }
}

public class GetPaymentByIdQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;

    public GetPaymentByIdQueryHandlerTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPayment()
    {
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var paymentResult = Payment.Create(orderId, tenantId, customerId, Money.Create(25.00m, "BRL").Value, PaymentMethod.Cash);
        var payment = paymentResult.Value;

        _mockPaymentRepository.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var query = new GetPaymentByIdQuery(paymentId);
        var handler = new GetPaymentByIdQueryHandler(_mockPaymentRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(payment.Id);
        result.Value.OrderId.Should().Be(orderId);
        result.Value.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task Handle_WithNonExistentPayment_ShouldFail()
    {
        _mockPaymentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var query = new GetPaymentByIdQuery(Guid.NewGuid());
        var handler = new GetPaymentByIdQueryHandler(_mockPaymentRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.NotFound");
    }
}

public class GetPaymentByOrderIdQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;

    public GetPaymentByOrderIdQueryHandlerTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPayment()
    {
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var paymentResult = Payment.Create(orderId, tenantId, customerId, Money.Create(30.00m, "BRL").Value, PaymentMethod.CardOnDelivery);
        var payment = paymentResult.Value;

        _mockPaymentRepository.Setup(x => x.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var query = new GetPaymentByOrderIdQuery(orderId);
        var handler = new GetPaymentByOrderIdQueryHandler(_mockPaymentRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(orderId);
        result.Value.Method.Should().Be(PaymentMethod.CardOnDelivery);
    }

    [Fact]
    public async Task Handle_WithNonExistentPayment_ShouldFail()
    {
        _mockPaymentRepository.Setup(x => x.GetByOrderIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var query = new GetPaymentByOrderIdQuery(Guid.NewGuid());
        var handler = new GetPaymentByOrderIdQueryHandler(_mockPaymentRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.NotFound");
    }
}

public class GetCartQueryHandlerTests
{
    private readonly Mock<ICartRepository> _mockCartRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public GetCartQueryHandlerTests()
    {
        _mockCartRepository = new Mock<ICartRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithExistingCart_ShouldReturnCart()
    {
        var cartResult = Cart.Create(_tenantId, _userId);
        var cart = cartResult.Value;

        var price = Money.Create(10.00m, "BRL").Value;
        cart.AddItem(Guid.NewGuid(), "Coca-Cola", price, 2);

        _mockCartRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var query = new GetCartQuery();
        var handler = new GetCartQueryHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(cart.Id);
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var query = new GetCartQuery();
        var handler = new GetCartQueryHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var query = new GetCartQuery();
        var handler = new GetCartQueryHandler(
            _mockCartRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cart.UserRequired");
    }
}

public class GetCustomerQueryHandlerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Mock<ICurrentUser> _mockCurrentUser;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public GetCustomerQueryHandlerTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
        _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnCustomer()
    {
        var email = Email.Create("test@example.com").Value;
        var customerResult = Customer.Create(_tenantId, _userId, "João Silva", email);
        var customer = customerResult.Value;

        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var query = new GetCustomerQuery();
        var handler = new GetCustomerQueryHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(customer.Id);
        result.Value.FullName.Should().Be("João Silva");
        result.Value.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var query = new GetCustomerQuery();
        var handler = new GetCustomerQueryHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithoutUser_ShouldFail()
    {
        _mockCurrentUser.Setup(x => x.UserId).Returns((Guid?)null);

        var query = new GetCustomerQuery();
        var handler = new GetCustomerQueryHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.UserRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldFail()
    {
        _mockCustomerRepository.Setup(x => x.GetByUserIdAsync(_tenantId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var query = new GetCustomerQuery();
        var handler = new GetCustomerQueryHandler(
            _mockCustomerRepository.Object,
            _mockTenantContext.Object,
            _mockCurrentUser.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NotFound");
    }
}

public class GetDeliverySettingsQueryHandlerTests
{
    private readonly Mock<IDeliverySettingsRepository> _mockDeliverySettingsRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetDeliverySettingsQueryHandlerTests()
    {
        _mockDeliverySettingsRepository = new Mock<IDeliverySettingsRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnSettings()
    {
        var settingsResult = DeliverySettings.Create(
            _tenantId,
            DeliveryFeeType.Fixed,
            Money.Create(5.00m, "BRL").Value);
        var settings = settingsResult.Value;

        _mockDeliverySettingsRepository.Setup(x => x.GetByTenantIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        var query = new GetDeliverySettingsQuery();
        var handler = new GetDeliverySettingsQueryHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(settings.Id);
        result.Value.FeeType.Should().Be(DeliveryFeeType.Fixed);
        result.Value.FixedFee.Should().Be(5.00m);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var query = new GetDeliverySettingsQuery();
        var handler = new GetDeliverySettingsQueryHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.TenantRequired");
    }

    [Fact]
    public async Task Handle_WithNonExistentSettings_ShouldFail()
    {
        _mockDeliverySettingsRepository.Setup(x => x.GetByTenantIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DeliverySettings?)null);

        var query = new GetDeliverySettingsQuery();
        var handler = new GetDeliverySettingsQueryHandler(
            _mockDeliverySettingsRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeliverySettings.NotFound");
    }
}

public class GetCategoryQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;

    public GetCategoryQueryHandlerTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnCategory()
    {
        var categoryId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var categoryResult = Category.Create(tenantId, "Bebidas", "Refrigerantes e sucos", null, 1);
        var category = categoryResult.Value;

        _mockCategoryRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var query = new GetCategoryQuery(categoryId);
        var handler = new GetCategoryQueryHandler(_mockCategoryRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(category.Id);
        result.Value.Name.Should().Be("Bebidas");
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldFail()
    {
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var query = new GetCategoryQuery(Guid.NewGuid());
        var handler = new GetCategoryQueryHandler(_mockCategoryRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.NotFound");
    }
}

public class GetCategoriesByTenantQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetCategoriesByTenantQueryHandlerTests()
    {
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnCategories()
    {
        var cat1 = Category.Create(_tenantId, "Bebidas", null, null, 1).Value;
        var cat2 = Category.Create(_tenantId, "Lanches", null, null, 2).Value;

        _mockCategoryRepository.Setup(x => x.GetByTenantIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { cat1, cat2 });

        var query = new GetCategoriesByTenantQuery();
        var handler = new GetCategoriesByTenantQueryHandler(
            _mockCategoryRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var query = new GetCategoriesByTenantQuery();
        var handler = new GetCategoriesByTenantQueryHandler(
            _mockCategoryRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.TenantRequired");
    }
}

public class GetProductQueryHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;

    public GetProductQueryHandlerTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnProduct()
    {
        var productId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var productResult = Product.Create(tenantId, "Coca-Cola 350ml", Money.Create(5.50m, "BRL").Value, null, "Refrigerante", null, 1);
        var product = productResult.Value;

        _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var query = new GetProductQuery(productId);
        var handler = new GetProductQueryHandler(_mockProductRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(product.Id);
        result.Value.Name.Should().Be("Coca-Cola 350ml");
        result.Value.Price.Should().Be(5.50m);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ShouldFail()
    {
        _mockProductRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var query = new GetProductQuery(Guid.NewGuid());
        var handler = new GetProductQueryHandler(_mockProductRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.NotFound");
    }
}

public class GetProductsByCategoryQueryHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;

    public GetProductsByCategoryQueryHandlerTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnProducts()
    {
        var categoryId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var prod1 = Product.Create(tenantId, "Coca-Cola", Money.Create(5.50m, "BRL").Value, categoryId, null, null, 1).Value;
        var prod2 = Product.Create(tenantId, "Guaraná", Money.Create(4.50m, "BRL").Value, categoryId, null, null, 2).Value;

        _mockProductRepository.Setup(x => x.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { prod1, prod2 });

        var query = new GetProductsByCategoryQuery(categoryId);
        var handler = new GetProductsByCategoryQueryHandler(_mockProductRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithEmptyCategory_ShouldReturnEmptyList()
    {
        var categoryId = Guid.NewGuid();

        _mockProductRepository.Setup(x => x.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        var query = new GetProductsByCategoryQuery(categoryId);
        var handler = new GetProductsByCategoryQueryHandler(_mockProductRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}

public class GetProductsByTenantQueryHandlerTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetProductsByTenantQueryHandlerTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockTenantContext = new Mock<ITenantContext>();
        _mockTenantContext.Setup(x => x.TenantId).Returns(_tenantId);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnProducts()
    {
        var prod1 = Product.Create(_tenantId, "Coca-Cola", Money.Create(5.50m, "BRL").Value, null, null, null, 1).Value;
        var prod2 = Product.Create(_tenantId, "Guaraná", Money.Create(4.50m, "BRL").Value, null, null, null, 2).Value;

        _mockProductRepository.Setup(x => x.GetByTenantIdAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { prod1, prod2 });

        var query = new GetProductsByTenantQuery();
        var handler = new GetProductsByTenantQueryHandler(
            _mockProductRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithoutTenant_ShouldFail()
    {
        _mockTenantContext.Setup(x => x.TenantId).Returns((Guid?)null);

        var query = new GetProductsByTenantQuery();
        var handler = new GetProductsByTenantQueryHandler(
            _mockProductRepository.Object,
            _mockTenantContext.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Product.TenantRequired");
    }
}

public class GetTenantQueryHandlerTests
{
    private readonly Mock<ITenantRepository> _mockTenantRepository;

    public GetTenantQueryHandlerTests()
    {
        _mockTenantRepository = new Mock<ITenantRepository>();
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnTenant()
    {
        var tenantId = Guid.NewGuid();

        var tenantResult = Tenant.Create("Restaurante ABC", "restaurante-abc", "O melhor restaurante", null);
        var tenant = tenantResult.Value;

        _mockTenantRepository.Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var query = new GetTenantQuery(tenantId);
        var handler = new GetTenantQueryHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(tenant.Id);
        result.Value.Name.Should().Be("Restaurante ABC");
        result.Value.Slug.Should().Be("restaurante-abc");
    }

    [Fact]
    public async Task Handle_WithNonExistentTenant_ShouldFail()
    {
        _mockTenantRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var query = new GetTenantQuery(Guid.NewGuid());
        var handler = new GetTenantQueryHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Tenant.NotFound");
    }
}

public class GetTenantsQueryHandlerTests
{
    private readonly Mock<ITenantRepository> _mockTenantRepository;

    public GetTenantsQueryHandlerTests()
    {
        _mockTenantRepository = new Mock<ITenantRepository>();
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnTenants()
    {
        var tenant1 = Tenant.Create("Restaurante A", "restaurante-a", null, null).Value;
        var tenant2 = Tenant.Create("Restaurante B", "restaurante-b", null, null).Value;

        _mockTenantRepository.Setup(x => x.GetAllAsync(1, 20, It.IsAny<string?>(), It.IsAny<TenantStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant> { tenant1, tenant2 });
        _mockTenantRepository.Setup(x => x.CountAsync(It.IsAny<string?>(), It.IsAny<TenantStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var query = new GetTenantsQuery(1, 20, null, null);
        var handler = new GetTenantsQueryHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithSearch_ShouldPassSearchToRepository()
    {
        var tenant = Tenant.Create("Restaurante A", "restaurante-a", null, null).Value;

        _mockTenantRepository.Setup(x => x.GetAllAsync(1, 20, "Restaurante", It.IsAny<TenantStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tenant> { tenant });
        _mockTenantRepository.Setup(x => x.CountAsync("Restaurante", It.IsAny<TenantStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var query = new GetTenantsQuery(1, 20, "Restaurante", null);
        var handler = new GetTenantsQueryHandler(_mockTenantRepository.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Name.Should().Be("Restaurante A");
    }
}
