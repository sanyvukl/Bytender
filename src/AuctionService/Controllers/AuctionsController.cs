using AuctionService.Data;
using AuctionService.Helpers;
using AuctionService.Models;
using AuctionService.Models.DTOs;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        this._mapper = mapper;
        this._context = context;
        this._publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAuctionsAsync(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x =>
                x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

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

    [Authorize]
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
            auction.Seller = User.Identity.Name;
            // #
            await _context.Auctions.AddAsync(auction);

            var auctionToReturn = _mapper.Map<AuctionDTO>(auction);
            var auctionToPublish = _mapper.Map<AuctionCreated>(auctionToReturn);
            // - Publish the Message
            await this._publishEndpoint.Publish(auctionToPublish);
            // # Publish the Message

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Couldn't save data to the DB");
            }


            return Created(nameof(GetAuctionByIdAsync), auctionToReturn);
        }
        catch (Exception e)
        {
            ExceptionHandler.HandleException(e, nameof(CreateAuctionAsync));
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuctionAsync(Guid id, UpdateAuctionDTO auctionDTO)
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
            System.Console.WriteLine("Got the Auction from the DB Successfully");
            if (auctionFromDb == null) return NotFound();
            // TODO: Check if the User == Seller
            if (auctionFromDb.Seller != User.Identity.Name) return Forbid();
            // #
            auctionFromDb.Item.Make = auctionDTO.Make ?? auctionFromDb.Item.Make;
            auctionFromDb.Item.Model = auctionDTO.Model ?? auctionFromDb.Item.Model;
            auctionFromDb.Item.Year = auctionDTO.Year ?? auctionFromDb.Item.Year;
            auctionFromDb.Item.Mileage = auctionDTO.Mileage ?? auctionFromDb.Item.Mileage;
            auctionFromDb.Item.Color = auctionDTO.Color ?? auctionFromDb.Item.Color;

            Console.WriteLine("Mapped the Auction from the DB Successfully");
            // - Publish the Message
            await this._publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auctionFromDb));
            // # Publish the Message
            Console.WriteLine("Published the Auction from the DB Successfully");

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem saving changes");
        }
        catch (Exception e)
        {
            ExceptionHandler.HandleException(e, nameof(UpdateAuctionAsync));
            return StatusCode(500, "Internal server error");
        }
    }

    [Authorize]
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
            if (auctionFromDb.Seller != User.Identity.Name) return Forbid();
            // #
            this._context.Auctions.Remove(auctionFromDb);

            // - Publish the Message
            await this._publishEndpoint.Publish<AuctionDeleted>(new { Id = auctionFromDb.Id.ToString() });
            // # Publish the Message

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
