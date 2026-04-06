using Dapper;
using SV22T1020136.DataLayers.Interfaces;
using SV22T1020136.Models.Sales;
using SV22T1020136.Models.Common;

namespace SV22T1020136.DataLayers.SQLServer
{
    public class OrderRepository : BaseSQLDAL, IOrderRepository
    {
        public OrderRepository(string connectionString) : base(connectionString) { }

        public async Task<int> AddAsync(Order data)
        {
            const string sql = @"INSERT INTO Orders
                (CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, AcceptTime, ShipperID, ShippedTime, FinishedTime, Status)
VALUES
                (@CustomerID, @OrderTime, @DeliveryProvince, @DeliveryAddress, @EmployeeID, @AcceptTime, @ShipperID, @ShippedTime, @FinishedTime, @Status);
SELECT CAST(SCOPE_IDENTITY() as int);";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var id = await cn.QuerySingleAsync<int>(sql, data);
            return id;
        }

        public async Task<bool> DeleteAsync(int orderID)
        {
            using var cn = GetConnection();
            await cn.OpenAsync();
            await cn.ExecuteAsync("DELETE FROM OrderDetails WHERE OrderID = @orderID", new { orderID });
            var affected = await cn.ExecuteAsync("DELETE FROM Orders WHERE OrderID = @orderID", new { orderID });
            return affected > 0;
        }

        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            const string sql = @"
SELECT o.OrderID, o.CustomerID, o.OrderTime, o.DeliveryProvince, o.DeliveryAddress,
       o.EmployeeID, o.AcceptTime, o.ShipperID, o.ShippedTime, o.FinishedTime, o.Status,
       c.CustomerName, c.ContactName AS CustomerContactName, c.Email AS CustomerEmail, c.Phone AS CustomerPhone, c.Address AS CustomerAddress,
       e.FullName AS EmployeeName,
       s.ShipperName, s.Phone AS ShipperPhone
FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
WHERE o.OrderID = @orderID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var item = await cn.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { orderID });
            return item;
        }

        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            const string sql = @"
SELECT od.OrderID, od.ProductID, od.Quantity, od.SalePrice,
       p.ProductName, p.Unit, p.Photo
FROM OrderDetails od
INNER JOIN Products p ON od.ProductID = p.ProductID
WHERE od.OrderID = @orderID
ORDER BY od.ProductID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var items = (await cn.QueryAsync<OrderDetailViewInfo>(sql, new { orderID })).ToList();
            return items;
        }

        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                whereClauses.Add("(c.CustomerName LIKE @q OR c.ContactName LIKE @q OR o.DeliveryAddress LIKE @q)");
                parameters.Add("q", "%" + input.SearchValue + "%");
            }
            if ((int)input.Status != 0)
            {
                whereClauses.Add("o.Status = @status");
                parameters.Add("status", (int)input.Status);
            }
            var where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

            string sql = $@"
SELECT o.OrderID, o.CustomerID, o.OrderTime, o.DeliveryProvince, o.DeliveryAddress,
       o.EmployeeID, o.AcceptTime, o.ShipperID, o.ShippedTime, o.FinishedTime, o.Status,
       c.CustomerName, c.ContactName AS CustomerContactName, c.Email AS CustomerEmail, c.Phone AS CustomerPhone, c.Address AS CustomerAddress,
       e.FullName AS EmployeeName,
       s.ShipperName, s.Phone AS ShipperPhone
FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
{where}
ORDER BY o.OrderID DESC";
            if (input.PageSize > 0)
            {
                sql += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            using var cn = GetConnection();
            await cn.OpenAsync();
            var items = (await cn.QueryAsync<OrderViewInfo>(sql, parameters)).ToList();

            // Get total count for paging
            var countSql = $@"
SELECT COUNT(*)
FROM Orders o
LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
{where}";
            var totalCount = await cn.QuerySingleAsync<int>(countSql, parameters);

            return new PagedResult<OrderViewInfo>
            {
                DataItems = items,
                RowCount = totalCount,
                PageSize = input.PageSize,
                Page = input.Page
            };
        }

        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            const string sql = @"
SELECT od.OrderID, od.ProductID, od.Quantity, od.SalePrice,
       p.ProductName, p.Unit, p.Photo
FROM OrderDetails od
INNER JOIN Products p ON od.ProductID = p.ProductID
WHERE od.OrderID = @orderID AND od.ProductID = @productID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var item = await cn.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(sql, new { orderID, productID });
            return item;
        }

        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            const string sql = @"INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SalePrice)
VALUES (@OrderID, @ProductID, @Quantity, @SalePrice);";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> UpdateAsync(Order data)
        {
            const string sql = @"UPDATE Orders
SET CustomerID = @CustomerID,
    OrderTime = @OrderTime,
    DeliveryProvince = @DeliveryProvince,
    DeliveryAddress = @DeliveryAddress,
    EmployeeID = @EmployeeID,
    AcceptTime = @AcceptTime,
    ShipperID = @ShipperID,
    ShippedTime = @ShippedTime,
    FinishedTime = @FinishedTime,
    Status = @Status
WHERE OrderID = @OrderID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            const string sql = "UPDATE OrderDetails SET Quantity = @Quantity, SalePrice = @SalePrice WHERE OrderID = @OrderID AND ProductID = @ProductID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            const string sql = "DELETE FROM OrderDetails WHERE OrderID = @orderID AND ProductID = @productID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, new { orderID, productID });
            return affected > 0;
        }
    }
}
