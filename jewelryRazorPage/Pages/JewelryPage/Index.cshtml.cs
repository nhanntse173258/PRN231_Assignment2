using BOs.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace jewelryRazorPage.Pages.JewelryPage
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IList<SilverJewelryDto> SilverJewelry { get; set; } = new List<SilverJewelryDto>();
        [BindProperty(SupportsGet = true)]
        public string SearchName { get; set; } = default!;
        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token))
            {
               return RedirectToPage("/AuthPage/Login");
            }
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var query = new List<string>();
                query.Add("$expand=Category");
                query.Add("$count=true");
                
                if (!string.IsNullOrEmpty(SearchName))
                {
                    query.Add($"$filter=contains(SilverJewelryName, '{SearchName}') or contains(cast(MetalWeight, 'Edm.String'), '{SearchName}')");
                }
                var queryString = string.Join("&", query);
                var response = await _httpClient.GetAsync($"http://localhost:5294/odata/Jewelry?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ODataResponse<SilverJewelryDto>>(content).Value;
                    SilverJewelry = result;
                }
                
                return Page();
            }
            else if (userRole == "Staff")
            {
                return Unauthorized();
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
