using Newtonsoft.Json;

namespace SV22T1020136.Shop
{
    public static class ApplicationContext
    {
        private static IHttpContextAccessor? _httpContextAccessor;
        private static IWebHostEnvironment? _webHostEnvironment;
        private static IConfiguration? _configuration;

        /// <summary>
        /// Thiết lập các dịch vụ cơ bản cho ApplicationContext.
        /// </summary>
        /// <param name="httpContextAccessor">Accessor để truy cập HttpContext (session, user...)</param>
        /// <param name="webHostEnvironment">Môi trường web (đường dẫn wwwroot, nội dung static...)</param>
        /// <param name="configuration">Cấu hình ứng dụng (appsettings)</param>
        /// <exception cref="ArgumentNullException">Bật nếu bất kỳ tham số nào là null.</exception>
        public static void Configure(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException();
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException();
            _configuration = configuration ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Trả về HttpContext hiện tại (nếu có). Dùng để truy cập Session, User, Request, Response...
        /// </summary>
        public static HttpContext? HttpContext => _httpContextAccessor?.HttpContext;

        /// <summary>
        /// Trả về đối tượng môi trường hosting (IWebHostEnvironment) để lấy thông tin như
        /// đường dẫn đến wwwroot hoặc kiểm tra môi trường (Development/Production).
        /// </summary>
        public static IWebHostEnvironment? WebHostEnvironment => _webHostEnvironment;

        /// <summary>
        /// Trả về IConfiguration được cấu hình cho ứng dụng (appsettings và các nguồn cấu hình khác).
        /// </summary>
        public static IConfiguration? Configuration => _configuration;

        /// <summary>
        /// Lưu một đối tượng vào Session dưới dạng chuỗi JSON.
        /// - Nếu đối tượng không null sẽ được serialize bằng JSON và lưu vào Session với khóa chỉ định.
        /// - Bắt lỗi bên trong để tránh làm vỡ luồng yêu cầu nếu serialization hoặc Session không khả dụng.
        /// </summary>
        /// <param name="key">Khóa dùng để lưu trên Session.</param>
        /// <param name="value">Đối tượng cần lưu. Có thể là bất kỳ kiểu nào có thể serialize thành JSON.</param>
        public static void SetSessionData(string key, object value)
        {
            try
            {
                // Chuyển đối tượng sang JSON trước khi lưu
                string sValue = JsonConvert.SerializeObject(value);

                // Nếu chuỗi JSON rỗng thì không lưu
                if (!string.IsNullOrEmpty(sValue))
                    _httpContextAccessor?.HttpContext?.Session.SetString(key, sValue);
            }
            catch
            {
                // Im lặng bắt lỗi để không làm gián đoạn luồng request khi session không khả dụng
            }
        }

        /// <summary>
        /// Đọc một đối tượng từ Session và deserialize từ JSON về kiểu <typeparamref name="T"/>.
        /// Trả về null nếu không có dữ liệu hoặc khi có lỗi (ví dụ JSON không hợp lệ).
        /// </summary>
        /// <typeparam name="T">Kiểu của đối tượng cần lấy. Phải là kiểu tham chiếu (class).</typeparam>
        /// <param name="key">Khóa dùng để đọc từ Session.</param>
        /// <returns>Đối tượng kiểu <typeparamref name="T"/> nếu tồn tại và deserialize thành công; ngược lại null.</returns>
        public static T? GetSessionData<T>(string key) where T : class
        {
            try
            {
                // Lấy chuỗi JSON từ Session
                string sValue = _httpContextAccessor?.HttpContext?.Session.GetString(key) ?? "";

                // Nếu chuỗi rỗng, trả về null
                if (!string.IsNullOrEmpty(sValue))
                    return JsonConvert.DeserializeObject<T>(sValue);
            }
            catch
            {
                // Bắt và bỏ qua lỗi deserialize/session để caller không bị crash
            }
            return null;
        }
    }
}
