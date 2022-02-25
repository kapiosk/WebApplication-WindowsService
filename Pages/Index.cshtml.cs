using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplicationService.Services;

namespace WebApplicationService.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public int Counter { get; set; }

        public IndexModel(CounterService counterService)
        {
            Counter = counterService.Counter();
        }

        public void OnGet()
        {

        }
    }
}