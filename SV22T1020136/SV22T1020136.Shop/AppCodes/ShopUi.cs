namespace SV22T1020136.Shop;

/// <summary>
/// H? tr? các h?ng s? và helper liên quan ??n giao di?n c?a hàng (paths, tên th??ng hi?u...).
/// Các giá tr? tr? v? ???c thi?t k? ?? dùng tr?c ti?p trong Razor views (???ng d?n t??ng ??i t?i wwwroot).
/// </summary>
public static class ShopUi
{
    /// <summary>
    /// Tên th??ng hi?u hi?n th? trên giao di?n.
    /// </summary>
    public static string BrandName => "MH SHOP";

    /// <summary>
    /// ?nh thay th? (placeholder) khi s?n ph?m không có ?nh.
    /// Giá tr? là ???ng d?n t??ng ??i ??n file trong th? m?c wwwroot.
    /// </summary>
    public static string ProductPlaceholder => "/images/product-placeholder.svg";

    /// <summary>
    /// Tr? v? ???ng d?n ?nh s?n ph?m ?? hi?n th? trên giao di?n.
    /// N?u tham s? <paramref name="photo"/> là null, r?ng ho?c ch? có kho?ng tr?ng,
    /// ph??ng th?c s? tr? v? ???ng d?n c?a ?nh placeholder.
    /// Ng??c l?i tr? v? ???ng d?n theo m?u "/images/products/{photo}".
    /// </summary>
    /// <param name="photo">
    /// Tên file ?nh s?n ph?m (ví d? "abc.jpg") ho?c null n?u không có ?nh.
    /// Không truy?n ???ng d?n ??y ??; ph??ng th?c s? sinh ???ng d?n t??ng ??i cho Razor view.
    /// </param>
    /// <returns>
    /// Chu?i ???ng d?n t??ng ??i t?i ?nh dùng trong th? &lt;img&gt; (ví d? "/images/products/abc.jpg"
    /// ho?c "/images/product-placeholder.svg" khi không có ?nh).
    /// </returns>
    public static string ProductImage(string? photo)
    {
        // N?u không có tên ?nh h?p l?, dùng placeholder
        if (string.IsNullOrWhiteSpace(photo))
            return ProductPlaceholder;

        // Tr? v? ???ng d?n t?i th? m?c ch?a ?nh s?n ph?m
        return $"/images/products/{photo}";
    }
}
