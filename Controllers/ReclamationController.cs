using Microsoft.AspNetCore.Mvc;
using WebApiTest.Interface;
using WebApiTest.Models;

namespace WebApiTest.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ReclamationController : ControllerBase
	{

		private readonly IReclamationRepository _reclamationRepository;
		public ReclamationController(IReclamationRepository reclamationRepository)
		{
			_reclamationRepository = reclamationRepository;
		}
		[HttpGet]
		[ProducesResponseType(200, Type = typeof(ICollection<MsReclamationweb>))]
		public IActionResult GetAllReclamations()
		{
			var reclamations = _reclamationRepository.GetReclamationwebs();
			return Ok(reclamations);
		}
		[HttpGet("{id}")]
		[ProducesResponseType(200, Type = typeof(ICollection<MsReclamationweb>))]
		public IActionResult GetOneReclamation(int id)
		{
			var reclamation = _reclamationRepository.GetReclamationweb(id);
			return Ok(reclamation);
		}
		[HttpPost]
		[ProducesResponseType(204)]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public IActionResult CreateReclamation([FromBody] MsReclamationweb createReclamation, string result_type, string client)
		{
			if (createReclamation == null)
				return BadRequest(ModelState);

			var reclamation = _reclamationRepository.GetReclamationwebs()
				.Where(c => c.Id == createReclamation.Id)
				.FirstOrDefault();

			if (reclamation != null)
			{
				ModelState.AddModelError("", "Reclamation already exists");
				return StatusCode(422, ModelState);
			}

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (!_reclamationRepository.CreateReclamation(createReclamation, result_type, client))
			{
				ModelState.AddModelError("", "Something went wrong while savin");
				return StatusCode(500, ModelState);
			}
			createReclamation.Date = DateTime.Now;
			return Ok(createReclamation);
		}


	}


}
