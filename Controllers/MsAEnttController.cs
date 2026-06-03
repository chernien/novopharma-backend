using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiTest.Interface;
using WebApiTest.Models;
using WebApiTest.Repository;

namespace WebApiTest.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MsAEnttController : ControllerBase
	{
		private readonly IMsEnttRepository _msEnttRepository;
		public MsAEnttController(IMsEnttRepository msEnttRepository)
		{
			_msEnttRepository = msEnttRepository;
		}

		[HttpGet("ligne/{dop}")]
		[ProducesResponseType(200, Type = typeof(ICollection<MsALigne>))]
		public IActionResult GetAllLigneDop(string dop)
		{
			var lignes = _msEnttRepository.GetLignesByDop(dop);
			return Ok(lignes);
		}

		[HttpGet("facture")]
		[ProducesResponseType(200, Type = typeof(ICollection<MsAEntt>))]
		public IActionResult GetAllEntsF(string code_client)
		{

			var ents = _msEnttRepository.GetMsAEnttsFacture(code_client);
			return Ok(ents);
		}

		[HttpGet("commande")]
		[ProducesResponseType(200, Type = typeof(ICollection<MsAEntt>))]
		public IActionResult GetAllEntsC(string code_client)
		{
			var ents = _msEnttRepository.GetMsAEnttsCommande(code_client);
			return Ok(ents);
		}
		[HttpGet("commande/mois")]
		[ProducesResponseType(200, Type = typeof(ICollection<MsAEntt>))]
		public IActionResult GetAllEntsCMois(string code_client)
		{
			var ents = _msEnttRepository.GetMsAEnttsCommandeMois(code_client);
			return Ok(ents);
		}
		[HttpGet("commande/trimestre")]
		[ProducesResponseType(200, Type = typeof(ICollection<MsAEntt>))]
		public IActionResult GetAllEntsCTrimestre(string code_client)
		{
			var ents = _msEnttRepository.GetMsAEnttsCommandeTrimestre(code_client);
			return Ok(ents);
		}
	}
}
