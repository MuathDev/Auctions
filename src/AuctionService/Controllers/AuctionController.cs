using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;
[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
     private readonly AuctionDbContext _dbcontext;
     private readonly IMapper _mapper;

     public AuctionController(AuctionDbContext dbContext, IMapper mapper)
     {
          _dbcontext = dbContext;
          _mapper = mapper;
     }
     [HttpGet]
     public async Task<ActionResult<List<AuctionDto>>> GetAllAuction()
     {
          var auctions = await _dbcontext.Auctions
              .Include(x => x.Item)
              .OrderBy(x => x.Item.Make)
              .ToListAsync();

          return _mapper.Map<List<AuctionDto>>(auctions);
     }
     [HttpGet("{id}")]
     public async Task<ActionResult<AuctionDto>> GetAuction(Guid id)
     {
          var auction = await _dbcontext.Auctions
          .Include(x => x.Item)
          .OrderBy(x => x.Item.Make)
          .FirstOrDefaultAsync(i => i.Id == id);
          if (auction == null) return NotFound();
          return _mapper.Map<AuctionDto>(auction);
     }
     [HttpPost]
     public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
     {
          var auction = _mapper.Map<Auction>(auctionDto);
          //TODO: Add Current user as seller
          auction.Seller = "Test";
          _dbcontext.Auctions.Add(auction);
          var result = await _dbcontext.SaveChangesAsync() > 0;
          if (!result) return BadRequest("Could not save changes to database");
          return CreatedAtAction(nameof(GetAuction),
             new { auction.Id }, _mapper.Map<AuctionDto>(auction));
     }
     [HttpPut("{id}")]
     public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
     {
          var auction = await _dbcontext.Auctions.Include(x => x.Item)
          .FirstOrDefaultAsync(a => a.Id == id);
          if (auction == null) return NotFound();

          //TODO: check seller == username
          auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
          auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
          auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
          // auction.Item.ImageUrl = updateAuctionDto.ImageUrl ?? auction.Item.ImageUrl;
          auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
          auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

          var result = await _dbcontext.SaveChangesAsync() > 0;
          if (result) return Ok();
          return BadRequest("Problem saving changes");
     }
     [HttpDelete("{id}")]
     public async Task<ActionResult> DeleteAuction(Guid id)
     {
          var auctions = await _dbcontext.Auctions.FindAsync(id);
          if (auctions == null) return NotFound();
          _dbcontext.Auctions.Remove(auctions);
          var result = await _dbcontext.SaveChangesAsync() > 0;
          if (!result) return BadRequest("Could not delete Auction");
          return Ok();
     }
}
