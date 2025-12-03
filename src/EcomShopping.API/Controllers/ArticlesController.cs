using EcomShopping.Domain.Entities;
using EcomShopping.Application.DTOs;
using EcomShopping.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomShopping.API.Controllers;

/// <summary>
/// API endpoints for article management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(ApplicationDbContext context, ILogger<ArticlesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get a paged list of published articles
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <returns>Paged list of articles</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetArticles(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (pageSize > 100) pageSize = 100;
            if (page < 1) page = 1;

            var query = _context.Articles
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var articles = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var articleDtos = articles.Select(MapToDto).ToList();

            return Ok(new
            {
                items = articleDtos,
                page,
                pageSize,
                totalCount,
                totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving articles");
            return StatusCode(500, "An error occurred while retrieving articles");
        }
    }

    /// <summary>
    /// Get a specific article by ID
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <returns>Article details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArticleDto>> GetArticle(int id)
    {
        try
        {
            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == id && a.IsPublished);

            if (article == null)
            {
                return NotFound($"Article with ID {id} not found");
            }

            return Ok(MapToDto(article));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving article {ArticleId}", id);
            return StatusCode(500, "An error occurred while retrieving the article");
        }
    }

    /// <summary>
    /// Get a specific article by slug
    /// </summary>
    /// <param name="slug">Article slug</param>
    /// <returns>Article details</returns>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArticleDto>> GetArticleBySlug(string slug)
    {
        try
        {
            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.Slug == slug && a.IsPublished);

            if (article == null)
            {
                return NotFound($"Article with slug '{slug}' not found");
            }

            return Ok(MapToDto(article));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving article by slug {Slug}", slug);
            return StatusCode(500, "An error occurred while retrieving the article");
        }
    }

    /// <summary>
    /// Create a new article
    /// </summary>
    /// <param name="dto">Article creation data</param>
    /// <returns>Created article</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArticleDto>> CreateArticle([FromBody] CreateArticleDto dto)
    {
        try
        {
            var slug = GenerateSlug(dto.Title);
            
            // Check if slug already exists
            if (await _context.Articles.AnyAsync(a => a.Slug == slug))
            {
                return BadRequest("An article with this title already exists");
            }

            var article = new Article
            {
                Title = dto.Title,
                Slug = slug,
                Content = dto.Content,
                Summary = dto.Summary,
                Author = dto.Author,
                ImageUrl = dto.ImageUrl,
                IsPublished = dto.IsPublished,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = dto.IsPublished ? DateTime.UtcNow : null
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, MapToDto(article));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating article");
            return StatusCode(500, "An error occurred while creating the article");
        }
    }

    /// <summary>
    /// Update an existing article
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <param name="dto">Article update data</param>
    /// <returns>Updated article</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArticleDto>> UpdateArticle(int id, [FromBody] UpdateArticleDto dto)
    {
        try
        {
            var article = await _context.Articles.FindAsync(id);

            if (article == null)
            {
                return NotFound($"Article with ID {id} not found");
            }

            var newSlug = GenerateSlug(dto.Title);
            
            // Check if new slug conflicts with another article
            if (newSlug != article.Slug && await _context.Articles.AnyAsync(a => a.Slug == newSlug))
            {
                return BadRequest("An article with this title already exists");
            }

            var wasPublished = article.IsPublished;

            article.Title = dto.Title;
            article.Slug = newSlug;
            article.Content = dto.Content;
            article.Summary = dto.Summary;
            article.Author = dto.Author;
            article.ImageUrl = dto.ImageUrl;
            article.IsPublished = dto.IsPublished;
            article.UpdatedAt = DateTime.UtcNow;

            // Set PublishedAt when article is published for the first time
            if (dto.IsPublished && !wasPublished)
            {
                article.PublishedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(MapToDto(article));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating article {ArticleId}", id);
            return StatusCode(500, "An error occurred while updating the article");
        }
    }

    /// <summary>
    /// Delete an article
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteArticle(int id)
    {
        try
        {
            var article = await _context.Articles.FindAsync(id);

            if (article == null)
            {
                return NotFound($"Article with ID {id} not found");
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting article {ArticleId}", id);
            return StatusCode(500, "An error occurred while deleting the article");
        }
    }

    private static ArticleDto MapToDto(Article article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            Slug = article.Slug,
            Content = article.Content,
            Summary = article.Summary,
            Author = article.Author,
            ImageUrl = article.ImageUrl,
            IsPublished = article.IsPublished,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt,
            PublishedAt = article.PublishedAt
        };
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");
        
        // Remove invalid characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        
        // Remove consecutive dashes
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        
        // Trim dashes from ends
        slug = slug.Trim('-');
        
        return slug;
    }
}
