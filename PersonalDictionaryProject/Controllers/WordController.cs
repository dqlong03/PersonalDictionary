﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalDictionaryProject.Dtos;
using PersonalDictionaryProject.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PersonalDictionaryProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WordController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public WordController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // Lấy danh sách từ của người dùng (private)
        [HttpGet("user")]
        public async Task<IActionResult> GetUserWords()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var words = await _context.Words.Where(w => w.UserId == userId).ToListAsync();
            return Ok(words);
        }
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminWords()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var words = await _context.Words.Where(w => w.IsPublic).ToListAsync();
            return Ok(words);
        }
        [HttpGet]
        public async Task<IActionResult> GetWordsById(int Id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var word = await _context.Words.Where(w => w.UserId == userId && w.Id == Id).ToListAsync();
            if (word == null) return NotFound();
            return Ok(word);
        }
        [HttpGet("admin/get")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminGetWordsById(int Id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var word = await _context.Words.Where(w => w.Id == Id).ToListAsync();
            if (word == null) return NotFound();
            return Ok(word);
        }
        [HttpPost]
        public async Task<IActionResult> AddWord([FromBody] WordDTO word)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (word.Example.Length == 0 || word.Definition.Length == 0 || word.WordText.Length == 0 || word.Language.Length == 0)
            {
                return BadRequest("Please fill in all fields");
            }
            Word newWord = new Word
            {
                Id = 0,
                WordText = word.WordText,
                Definition = word.Definition,
                Example = word.Example,
                Language = word.Language,
                UserId = userId,
                IsPublic = word.IsPublic,
                IsApproved = false
            };
            _context.Words.Add(newWord);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserWords), new { id = newWord.Id }, word);
        }
        [HttpPost("addPrivateWord")]
        public async Task<IActionResult> AddWordPrivate([FromBody] int wordId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            Word word = await _context.Words.FirstOrDefaultAsync(w => w.Id == wordId);
            var newWord = new Word
            {
                Id = 0,
                WordText = word.WordText,
                Definition = word.Definition,
                Example = word.Example,
                Language = word.Language,
                UserId = userId,
                IsPublic = false,
                IsApproved = false
            };
            _context.Words.Add(newWord);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserWords), new { id = newWord.Id }, word);
        }
        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAddWord([FromBody] WordDTO word)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (word.Example.Length == 0|| word.Definition.Length == 0 || word.WordText.Length == 0 || word.Language.Length == 0)
            {
                return BadRequest("Please fill in all fields");
            }
            Word newWord = new Word
            {
                Id = 0,
                WordText = word.WordText,
                Definition = word.Definition,
                Example = word.Example,
                Language = word.Language,
                UserId = userId,
                IsPublic = true,
                IsApproved = true
            };
            _context.Words.Add(newWord);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserWords), new { id = newWord.Id }, word);
        }
        [HttpPost("admin/bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAddWordsBulk([FromBody] List<WordDTO> words)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (words == null || words.Count == 0)
            {
                return BadRequest("No words to add.");
            }

            var newWords = new List<Word>();

            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word.WordText) ||
                    string.IsNullOrWhiteSpace(word.Definition) ||
                    string.IsNullOrWhiteSpace(word.Example) ||
                    string.IsNullOrWhiteSpace(word.Language))
                {
                    return BadRequest("Please fill in all fields for every word.");
                }

                Word newWord = new Word
                {
                    Id = 0,
                    WordText = word.WordText,
                    Definition = word.Definition,
                    Example = word.Example,
                    Language = word.Language,
                    UserId = userId,
                    IsPublic = true,
                    IsApproved = true
                };

                newWords.Add(newWord);
            }

            await _context.Words.AddRangeAsync(newWords);
            await _context.SaveChangesAsync();

            return Created(nameof(AdminAddWordsBulk), $"{newWords.Count} words added successfully.");
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchWords([FromQuery] string? query, [FromQuery] string? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var words = _context.Words.Where(w => w.UserId == userId).AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                words = words.Where(w => w.WordText.Contains(query) || w.Definition.Contains(query));
            }

            if (!string.IsNullOrEmpty(status))
            {
                words = status switch
                {
                    "public" => words.Where(w => w.IsPublic && w.IsApproved),
                    "pending" => words.Where(w => w.IsPublic && !w.IsApproved),
                    "private" => words.Where(w => !w.IsPublic && !w.IsApproved),
                    _ => words
                };
            }

            return Ok(await words.ToListAsync());
        }
        [HttpGet("searchOne")]
        public async Task<IActionResult> SearchWord([FromQuery] string? query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var words = _context.Words.Where(w => w.IsPublic && w.IsApproved).AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                words = words.Where(w => w.WordText.Equals(query));
            }

            var result = await words.ToListAsync();

            if (result.Count == 0)
            {
                return NotFound("No words found for the given query.");
            }

            return Ok(result);
        }

        [HttpGet("searchAdmin")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> SearchAdminWords([FromQuery] string? query, [FromQuery] string? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var words = _context.Words.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                words = words.Where(w => w.WordText.Contains(query) || w.Definition.Contains(query));
            }

            if (!string.IsNullOrEmpty(status))
            {
                words = status switch
                {
                    "public" => words.Where(w => w.IsPublic && w.IsApproved),
                    "pending" => words.Where(w => w.IsPublic && !w.IsApproved),
                    "private" => words.Where(w => !w.IsPublic && !w.IsApproved),
                    _ => words
                };
            }

            return Ok(await words.ToListAsync());
        }
        [HttpPut("upload")]
        public async Task<IActionResult> UploadWord([FromBody] WordDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var word = await _context.Words.FirstOrDefaultAsync(w => w.Id == model.Id && w.UserId == userId);
            var check = false;
            if(model.IsPublic == true)
            {
                var wordcheck = await _context.Words.FirstOrDefaultAsync(w => w.Id == model.Id && w.UserId == userId);
                if (wordcheck == null) return NotFound();
                if (wordcheck.IsApprovedYet == true)
                {
                    check = true;
                }
            }
            if (word == null) return NotFound();
            word.WordText = model.WordText;
            word.Definition = model.Definition;
            word.Example = model.Example;
            word.Language = model.Language;
            word.IsPublic = model.IsPublic;
            if(check == true)
            {
                word.IsApproved = true;
            }
            else
            {
                word.IsApproved = false;
            }
            await _context.SaveChangesAsync();
            return Ok("Word submitted for approval");
        }
        [HttpPut("uploadAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminUploadWord([FromBody] WordDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var word = await _context.Words.FirstOrDefaultAsync(w => w.Id == model.Id);

            if (word == null) return NotFound();
            word.WordText = model.WordText;
            word.Definition = model.Definition;
            word.Example = model.Example;
            word.Language = model.Language;
            word.IsPublic = true;
            word.IsApproved = true;
            word.IsApprovedYet = true;
            await _context.SaveChangesAsync();
            return Ok("Word submitted for approval");
        }
        [HttpPut("deniedAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeniedWord([FromBody] WordDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var word = await _context.Words.FirstOrDefaultAsync(w => w.Id == model.Id && w.UserId == userId);

            if (word == null) return NotFound();
            word.WordText = model.WordText;
            word.Definition = model.Definition;
            word.Example = model.Example;
            word.Language = model.Language;
            word.IsPublic = false;
            word.IsApproved = false;
            word.IsApprovedYet = false;
            await _context.SaveChangesAsync();
            return Ok("Word submitted for approval");
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteWord([FromBody]int Id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var word = await _context.Words.FirstOrDefaultAsync(w => w.Id == Id && w.UserId == userId);
            if (word == null) return NotFound();
            _context.Words.Remove(word);
            await _context.SaveChangesAsync();
            return Ok("Word deleted");
        }

        [HttpDelete("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeleteWord([FromBody] int Id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var word = await _context.Words.FirstOrDefaultAsync(w => w.Id == Id && w.UserId == userId);
            if (word == null) return NotFound();
            _context.Words.Remove(word);
            await _context.SaveChangesAsync();
            return Ok("Word deleted");
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedWords(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? query = null,
        [FromQuery] string? status = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var wordsQuery = _context.Words
                .Where(w => w.UserId == userId)
                .AsQueryable();

            // Lọc theo từ khóa
            if (!string.IsNullOrEmpty(query))
            {
                wordsQuery = wordsQuery.Where(w => w.WordText.Contains(query) || w.Definition.Contains(query));
            }

            // Lọc theo trạng thái (Public/Private/Approved/Pending)
            if (!string.IsNullOrEmpty(status))
            {
                status = status.ToLower();
                if (status == "public") wordsQuery = wordsQuery.Where(w => w.IsPublic);
                else if (status == "private") wordsQuery = wordsQuery.Where(w => !w.IsPublic);
                else if (status == "approved") wordsQuery = wordsQuery.Where(w => w.IsApproved);
                else if (status == "pending") wordsQuery = wordsQuery.Where(w => !w.IsApproved);
            }

            var totalItems = await wordsQuery.CountAsync();
            var words = await wordsQuery
                .OrderBy(w => w.WordText)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new
            {
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Words = words
            };

            return Ok(response);
        }

    }
}