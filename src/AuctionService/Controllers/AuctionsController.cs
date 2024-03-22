using AuctionService.Data;
using AuctionService.Helpers;
using AuctionService.Models;
using AuctionService.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        this._context = context;
        this._mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAuctionsAsync()
    {
        // Add secutiry checks here
        bool hasAccess = true;

        try
        {
            if (!hasAccess)
            {
                return Unauthorized();
            }

            var auctionsFromDb = await _context.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();
            if (auctionsFromDb == null)
            {
                return NotFound();
            }

            var auctionsDTO = _mapper.Map<List<AuctionDTO>>(auctionsFromDb);
            return Ok(auctionsDTO);
        }
        catch (Exception e)
        {
            ExceptionHandler.HandleException(e, nameof(GetAuctionsAsync));
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDTO>> GetAuctionByIdAsync(Guid id)
    {
        // Add secutiry checks here
        bool hasAccess = true;

        try
        {
            if (!hasAccess)
            {
                return Unauthorized();
            }

            var auctionFromDb = await _context.Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

            if (auctionFromDb == null)
            {
                return NotFound();
            }

            var auctionDTO = _mapper.Map<AuctionDTO>(auctionFromDb);
            return Ok(auctionDTO);
        }
        catch (Exception e)
        {
            ExceptionHandler.HandleException(e, nameof(GetAuctionByIdAsync));
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateAuctionAsync([FromBody] CreateAuctionDTO auctionDTO)
    {
        // Add secutiry checks here
        bool hasAccess = true;

        try
        {
            if (!hasAccess)
            {
                return Unauthorized();
            }

            var auction = _mapper.Map<Auction>(auctionDTO);
            // TODO: Add the user as a Seller
            auction.Seller = "Seller";
            // #
            await _context.Auctions.AddAsync(auction);
            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Couldn't save data to the DB");
            }

            var auctionToReturn = _mapper.Map<AuctionDTO>(auction);
            return Created(nameof(GetAuctionByIdAsync), auctionToReturn);
        }
        catch (Exception e)
        {
            ExceptionHandler.HandleException(e, nameof(CreateAuctionAsync));
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuctionAsync(Guid id, [FromBody] UpdateAuctionDTO auctionDTO)
    {
        // Add secutiry checks here
        bool hasAccess = true;

        try
        {
            if (!hasAccess)
            {
                return Unauthorized();
            }

            var auctionFromDb = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
            if (auctionFromDb == null)
            {
                return NotFound();
            }
            // TODO: Check if the User == Seller

            // #
            _mapper.Map(auctionDTO, auctionFromDb);

            auctionFromDb.Item.Make = auctionDTO.Make ?? auctionFromDb.Item.Make;
            auctionFromDb.Item.Model = auctionDTO.Model ?? auctionFromDb.Item.Model;
            auctionFromDb.Item.Year = auctionDTO.Year ?? auctionFromDb.Item.Year;
            auctionFromDb.Item.Mileage = auctionDTO.Mileage ?? auctionFromDb.Item.Mileage;
            auctionFromDb.Item.Color = auctionDTO.Color ?? auctionFromDb.Item.Color;
            auctionFromDb.Item.ImageUrl = auctionDTO.ImageUrl ?? auctionFromDb.Item.ImageUrl;

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Couldn't save data to the DB");
            }

            return Ok(auctionFromDb);
        }
        catch (Exception e)
        {
            ExceptionHandler.HandleException(e, nameof(UpdateAuctionAsync));
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuctionAsync(Guid id)
    {
        // Add secutiry checks here
        bool hasAccess = true;

        try
        {
            if (!hasAccess)
            {
                return Unauthorized();
            }

            var auctionFromDb = await _context.Auctions.FindAsync(id);
            if (auctionFromDb == null)
            {
                return NotFound();
            }
            // TODO: Check if the User == Seller

            // #
            _context.Auctions.Remove(auctionFromDb);
            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Couldn't delete data");
            }

            return NoContent();
        }
        catch (Exception e)
        {
            ExceptionHandler.HandleException(e, nameof(DeleteAuctionAsync));
            return StatusCode(500, "Internal server error");
        }
    }
}
