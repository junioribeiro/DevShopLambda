using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Extensions;

namespace Cadastrador.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {

        [HttpPost]
        public async Task PostAsync([FromBody] Pedido pedido)
        {
            await pedido.SalvarAsync();
        }
    }
}
