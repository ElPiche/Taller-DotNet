﻿using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quality_Management.DataAccess;
using Quality_Management.DTO;
using Quality_Management.Model;
using Quality_Management.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Quality_Management.Controllers
{

    [ApiController]
    [Route("quality-management-api")]
    public class QualityManagementController : Controller
    {
        private readonly IProcedureService _procedureService;
        private readonly IRealTimeMetricsService _realTimeMetricsService;
        private readonly IOfficeRepository _officeRepository;

        public QualityManagementController(IProcedureService procedureService, 
            IRealTimeMetricsService realTimeMetricsService, IOfficeRepository officeRepository)
        {
            _procedureService = procedureService;
            _realTimeMetricsService = realTimeMetricsService;
            _officeRepository = officeRepository;
        }
        
        [HttpPost]
        [Route("startProcedure")]
        public async Task<ActionResult<long>> CreateProcedure(ProcedureDTO procedure)
        {
            try
            {
                await _realTimeMetricsService.SendMetric(_realTimeMetricsService.ClientLeavesTheQueue, 
                    procedure.OfficeId);
                long id = await _procedureService.CreateProcedure(procedure);
                return Ok(id);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest("Fallo al crear tramite: " + ex);
            }
            catch (DbUpdateException ex)
            {
                return Conflict("Fallo al crear el tramite: " + ex);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error inesperado: " + ex.Message);
            }

        }


        [HttpPut]
        [Route("finishProcedure/{Id}")]
        public async Task<ActionResult> FinishProcedure(long Id, [FromBody] DateTime ProcedureEnd)
        {
            try
            {
                await _procedureService.EndProcedure(Id, ProcedureEnd);
                
                await _realTimeMetricsService.SendMetric(_realTimeMetricsService.PositionReleased,
                    _officeRepository.FindByProcedure(Id).OfficeId);
                
                return Ok("Tramite finalizado con exito. ");
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest("Fallo al finalizar: " + ex);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
            catch (DbUpdateConcurrencyException ex)
            {
               return Conflict("Fallo al finalizar: " + ex);
            }
            catch (DbUpdateException ex)
            {
                return Conflict("Fallo al finalizar: " + ex);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error inesperado: " + ex.Message);
            }
        }


        [HttpGet]
        [Route("getProcedure/{ProcedureId}")]
        public async Task<ActionResult<ProcedureDTO>> getProcedure(long ProcedureId)
        {
            try
            {
                var procedure = await _procedureService.GetProcedure(ProcedureId);
                return Ok(procedure);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest("Fallo al obtener: " + ex);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocurrió un error inesperado: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("client-registration/{officeId}")]
        public async Task<ActionResult> ClientRegistration(string officeId)
        {
            try
            {
                await _realTimeMetricsService.SendMetric(_realTimeMetricsService.ClientEnterTheQueue, officeId);
                return Ok();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"Error al enviar metrica: {e.Message}");
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        [Route("create-office")]
        public async Task<ActionResult<OfficeDTO>> CreateOffice([FromBody] OfficeDTO? office)
        {
            if (office == null) return BadRequest("La oficina no puede ser NULL");

            try
            {
                office = await _officeRepository.Save(office);
                return Ok(office);
                
            }
            catch(Exception e) when (e is DbUpdateException or DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Ha ocurrido un error a la hora de guardar los datos de la oficina");
            }
        }

        [HttpDelete]
        [Route("delete-office/{officeId}")]
        public async Task<ActionResult> DeleteOffice(string officeId)
        {
            try
            {
                await _officeRepository.Delete(_officeRepository.FindById(officeId));
                return Ok();
            }
            catch (Exception e) when (e is DbUpdateException or DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Ha ocurrido un error a la hora de eliminar los datos de la oficina");
            }
            catch (ArgumentException e)
            {
                return BadRequest($"No existe una oficina con id: {officeId}");
            }
        }
    }
}



/*
[HttpDelete]
[Route("deleteProcedure/{ProcedureId}")]
public async Task<ActionResult> deleteProcedure(long ProcedureId)
{

    try
    {
        await _procedureService.DeleteProcedure(ProcedureId);
        return Ok("Tramite Eliminado");
    }
    catch (ArgumentNullException ex)
    {
        return BadRequest("Fallo al eliminar: " + ex);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        return Conflict("Fallo al eliminar: " + ex);
    }
    catch (DbUpdateException ex)
    {
        return Conflict("Fallo al eliminar: " + ex);
    }
    catch (Exception ex)
    {
        return StatusCode(500, "Ocurrió un error inesperado: " + ex.Message);
    }

}

[HttpGet]
[Route("getProcedure/{ProcedureId}")]
public async Task<ActionResult<ProcedureDTO>> getProcedure(long ProcedureId)
{
    try
    {
        var procedure = await _procedureService.GetProcedure(ProcedureId);
        return Ok(procedure);
    }
    catch (ArgumentNullException ex)
    {
        return BadRequest("Fallo al obtener: " + ex);
    }
    catch (Exception ex)
    {
        return StatusCode(500, "Ocurrió un error inesperado: " + ex.Message);
    }
}

[HttpGet]
[Route("getProcedures")]
public async Task<ActionResult<IList<ProcedureDTO>>> getProcedures()
{
    try
    {
        var procedures = await _procedureService.GetAll();
        return Ok(procedures);
    }
    catch (Exception ex)
    {
        return StatusCode(500, "Ocurrió un error inesperado: " + ex.Message);
    }
}*/
