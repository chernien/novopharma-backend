using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiTest.Interface;
using WebApiTest.Models;
using WebApiTest.Repository;

namespace WebApiTest.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MsContactController : ControllerBase
	{
		private readonly IMs_ContactRepository _contactRepository;
		public MsContactController(IMs_ContactRepository ms_ContactRepository) {
			_contactRepository = ms_ContactRepository;
		}
		[HttpGet]
		[ProducesResponseType(200, Type = typeof(ICollection<Ms_Contactsite>))]
		public IActionResult GetAllMs_Contactsites()
		{
			var reclamations = _contactRepository.GetContactsites();
			return Ok(reclamations);
		}
		[HttpGet("{id}")]
		[ProducesResponseType(200, Type = typeof(ICollection<Ms_Contactsite>))]
		public IActionResult GetOneMs_Contactsite(int id)
		{
			var contact = _contactRepository.GetMs_Contactsite(id);
			return Ok(contact);
		}
		[HttpPost]
		[ProducesResponseType(204)]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		public IActionResult CreateContactsite([FromBody] Ms_Contactsite createContact)
		{
			if (createContact == null)
				return BadRequest(ModelState);

			var contact = _contactRepository.GetContactsites()
				.Where(c => c.Id == createContact.Id)
				.FirstOrDefault();

			if (contact != null)
			{
				ModelState.AddModelError("", "Contact already exists");
				return StatusCode(422, ModelState);
			}

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (!_contactRepository.CreateContact(createContact))
			{
				ModelState.AddModelError("", "Something went wrong while savin");
				return StatusCode(500, ModelState);
			}
			createContact.DATECR = DateTime.Now;
			return Ok(createContact);
		}


	}
}

