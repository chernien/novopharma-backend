using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WebApiTest.Data;
using WebApiTest.Models;

namespace WebApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FDocLigneController : ControllerBase
    {
        private readonly NOVOPHARMAContext _context;

        public FDocLigneController(NOVOPHARMAContext context)
        {
            _context = context;
        }

        [HttpGet("getDocLigneWithLotSerie")]
        public IActionResult GetDocLigneWithLotSerie()
        {
            try
            {
                var result = (from fdl in _context.F_Docligne
                              join fls in _context.F_Lotserie
                              on fdl.AR_Ref equals fls.AR_Ref into lotseries 
                              from fls in lotseries.DefaultIfEmpty()
                              where fdl.DO_Type == 3
                              group new { fdl, fls } by new
                              {
                                  fdl.AR_Ref,
                                  fdl.DO_Type,
                                  fdl.DO_Piece,
                                  fdl.DO_Date,
                                  fdl.CT_Num,
                                  fdl.EU_Qte,
                                  fls.LS_NoSerie
                              } into groupedData
                              select new
                              {
                                  groupedData.Key.AR_Ref,
                                  groupedData.Key.DO_Type,
                                  groupedData.Key.DO_Piece,
                                  groupedData.Key.DO_Date,
                                  groupedData.Key.CT_Num,
                                  groupedData.Key.EU_Qte,
                                  groupedData.Key.LS_NoSerie
                              })
                             .Take(1000)
                             .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return StatusCode(500, "Une erreur est survenue lors de l'exécution.");
            }
        }

        [HttpGet("getDocLigneWithLotSerieByDOPiece/{doPiece}")]
        public IActionResult GetDocLigneWithLotSerieByDOPiece(string doPiece)
        {
            try
            {
                var result = (from fdl in _context.F_Docligne
                              join fls in _context.F_Lotserie
                              on fdl.AR_Ref equals fls.AR_Ref
                              where fdl.DO_Piece == doPiece
                              select new
                              {
                                  fdl.CT_Num,
                                  fdl.DO_Piece,
                                  fdl.DO_Date,
                                  fdl.AR_Ref,
                                  fdl.EU_Qte,
                                  fls.LS_NoSerie
                              })
                             .Take(1000) 
                             .ToList();

                if (!result.Any())
                {
                    return NotFound($"Aucun résultat trouvé pour DO_PIECE : {doPiece}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return StatusCode(500, "Une erreur est survenue lors de l'exécution.");
            }
        }


    }
}
