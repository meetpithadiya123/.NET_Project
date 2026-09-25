-- ============================================================================
-- SQL Server DDL Script for UrbanCart Order Persistence
-- Table 1: tbl_order
-- Table 2: tbl_order_item
-- Target Database: ecommerce_website (or any Azure SQL / SQL Server instance)
-- ============================================================================

-- 1. Create tbl_order table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_order')
BEGIN
    CREATE TABLE [dbo].[tbl_order] (
        [OrderId] INT IDENTITY(1,1) NOT NULL,
        [CustomerId] INT NOT NULL,
        [OrderDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [TotalAmount] DECIMAL(18,2) NOT NULL,
        [OrderStatus] NVARCHAR(50) NOT NULL DEFAULT 'Placed',
        [ShippingAddress] NVARCHAR(255) NOT NULL,
        [City] NVARCHAR(100) NOT NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [Phone] NVARCHAR(25) NOT NULL,
        [ShippingEmail] NVARCHAR(255) NULL,
        [TransactionId] NVARCHAR(100) NOT NULL,
        [PaymentMode] NVARCHAR(50) NOT NULL,
        [PaymentStatus] NVARCHAR(50) NOT NULL DEFAULT 'Success',
        CONSTRAINT [PK_tbl_order] PRIMARY KEY CLUSTERED ([OrderId] ASC),
        CONSTRAINT [FK_tbl_order_tbl_customer_CustomerId] FOREIGN KEY ([CustomerId]) 
            REFERENCES [dbo].[tbl_customer] ([customer_id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_tbl_order_CustomerId] ON [dbo].[tbl_order] ([CustomerId] ASC);
    CREATE NONCLUSTERED INDEX [IX_tbl_order_OrderDate] ON [dbo].[tbl_order] ([OrderDate] DESC);
END
GO

-- 2. Create tbl_order_item table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tbl_order_item')
BEGIN
    CREATE TABLE [dbo].[tbl_order_item] (
        [OrderItemId] INT IDENTITY(1,1) NOT NULL,
        [OrderId] INT NOT NULL,
        [ProductId] INT NOT NULL,
        [ProductName] NVARCHAR(200) NOT NULL,
        [UnitPrice] DECIMAL(18,2) NOT NULL,
        [Quantity] INT NOT NULL,
        [SubTotal] DECIMAL(18,2) NOT NULL,
        CONSTRAINT [PK_tbl_order_item] PRIMARY KEY CLUSTERED ([OrderItemId] ASC),
        CONSTRAINT [FK_tbl_order_item_tbl_order_OrderId] FOREIGN KEY ([OrderId]) 
            REFERENCES [dbo].[tbl_order] ([OrderId]) ON DELETE CASCADE,
        CONSTRAINT [FK_tbl_order_item_tbl_product_ProductId] FOREIGN KEY ([ProductId]) 
            REFERENCES [dbo].[tbl_product] ([product_id]) ON DELETE NO ACTION
    );

    CREATE NONCLUSTERED INDEX [IX_tbl_order_item_OrderId] ON [dbo].[tbl_order_item] ([OrderId] ASC);
    CREATE NONCLUSTERED INDEX [IX_tbl_order_item_ProductId] ON [dbo].[tbl_order_item] ([ProductId] ASC);
END
GO
