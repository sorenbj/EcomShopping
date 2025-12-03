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
    /// Get a paged list of articles
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <param name="includeUnpublished">Include unpublished articles (default: false)</param>
    /// <returns>Paged list of articles</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetArticles(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeUnpublished = false)
    {
        try
        {
            if (pageSize > 100) pageSize = 100;
            if (page < 1) page = 1;

            var query = _context.Articles.AsQueryable();
            
            if (!includeUnpublished)
            {
                query = query.Where(a => a.IsPublished);
            }
            
            query = query.OrderByDescending(a => a.PublishedAt ?? a.CreatedAt);

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
    /// <param name="includeUnpublished">Include unpublished articles (default: false)</param>
    /// <returns>Article details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArticleDto>> GetArticle(int id, [FromQuery] bool includeUnpublished = false)
    {
        try
        {
            var query = _context.Articles.Where(a => a.Id == id);
            
            if (!includeUnpublished)
            {
                query = query.Where(a => a.IsPublished);
            }
            
            var article = await query.FirstOrDefaultAsync();

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

    /// <summary>
    /// Seed sample articles (for testing and demo purposes)
    /// </summary>
    /// <returns>Number of articles created</returns>
    [HttpPost("seed")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> SeedArticles()
    {
        try
        {
            // Check if articles already exist
            if (await _context.Articles.AnyAsync())
            {
                return Ok(new { message = "Articles already exist. Seed skipped.", count = 0 });
            }

            var sampleArticles = new List<Article>
            {
                new Article
                {
                    Title = "Welcome to Our E-commerce Platform",
                    Slug = "welcome-to-our-ecommerce-platform",
                    Content = "We are excited to announce the launch of our new e-commerce platform!\n\nOur platform offers a wide range of products with an intuitive shopping experience. Whether you're looking for electronics, clothing, or home goods, we have something for everyone.\n\nKey Features:\n- Easy-to-use product catalog with advanced search and filtering\n- Secure checkout process\n- Real-time inventory management\n- Fast and reliable shipping\n\nThank you for choosing us as your shopping destination. We look forward to serving you!",
                    Summary = "Discover our new e-commerce platform with a wide range of products and an intuitive shopping experience.",
                    Author = "Admin Team",
                    ImageUrl = "https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?w=800",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    PublishedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Article
                {
                    Title = "How to Find the Best Deals",
                    Slug = "how-to-find-the-best-deals",
                    Content = "Shopping smart means finding the best deals without compromising on quality. Here are some tips to help you save money:\n\n1. Sign up for our newsletter to get exclusive offers and early access to sales.\n2. Check our deals section regularly for limited-time promotions.\n3. Use filters to compare prices and find the best value.\n4. Read product reviews to ensure you're getting quality items.\n5. Take advantage of bulk purchase discounts.\n\nRemember, the best deal is not always the cheapest price - it's about getting the best value for your money. Happy shopping!",
                    Summary = "Learn how to shop smart and find the best deals on our platform with these helpful tips.",
                    Author = "Shopping Expert",
                    ImageUrl = "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=800",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    PublishedAt = DateTime.UtcNow.AddDays(-20)
                },
                new Article
                {
                    Title = "Understanding Our Return Policy",
                    Slug = "understanding-our-return-policy",
                    Content = "We want you to be completely satisfied with your purchase. That's why we offer a flexible return policy.\n\nReturn Policy Highlights:\n- 30-day return window for most items\n- Free returns on eligible products\n- Easy return process through our website\n- Full refund or exchange available\n\nHow to Return an Item:\n1. Log into your account and go to Order History\n2. Select the item you want to return\n3. Choose a return reason and follow the instructions\n4. Print the return label and ship the item back\n5. Receive your refund within 5-7 business days after we receive the item\n\nFor more information, please contact our customer support team.",
                    Summary = "Learn about our flexible return policy and how easy it is to return items if you're not satisfied.",
                    Author = "Customer Service",
                    ImageUrl = "https://images.unsplash.com/photo-1556740758-90de374c12ad?w=800",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    PublishedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Article
                {
                    Title = "New Product Categories Available",
                    Slug = "new-product-categories-available",
                    Content = "We're excited to announce the addition of new product categories to our store!\n\nNew Categories:\n- Home & Garden: Transform your living space with our curated selection of home decor and gardening supplies\n- Sports & Outdoors: Get active with our range of sporting goods and outdoor equipment\n- Books & Media: Explore our collection of books, music, and movies\n- Health & Beauty: Discover products to help you look and feel your best\n\nEach category features carefully selected products from trusted brands. We're continuously expanding our inventory to meet your needs.\n\nVisit our store today to explore these new categories and find your next favorite product!",
                    Summary = "Explore our newly added product categories including Home & Garden, Sports & Outdoors, Books & Media, and Health & Beauty.",
                    Author = "Product Team",
                    ImageUrl = "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=800",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    PublishedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Article
                {
                    Title = "Customer Spotlight: Success Stories",
                    Slug = "customer-spotlight-success-stories",
                    Content = "We love hearing from our customers! Here are some recent success stories from shoppers who found exactly what they needed.\n\nJohn's Home Office Transformation:\n\"I was able to find everything I needed to set up my home office in one place. The product quality exceeded my expectations, and the delivery was super fast!\"\n\nSarah's Gift Shopping Experience:\n\"Shopping for gifts has never been easier. The search filters helped me find the perfect presents for my family, and they all loved them!\"\n\nMike's Tech Upgrade:\n\"I was hesitant to buy electronics online, but the detailed product descriptions and customer reviews gave me confidence. My new laptop arrived quickly and works perfectly!\"\n\nThank you to all our customers for sharing your experiences. Your feedback helps us improve and serve you better!",
                    Summary = "Read inspiring stories from our satisfied customers and learn how they found the perfect products for their needs.",
                    Author = "Community Manager",
                    ImageUrl = "https://images.unsplash.com/photo-1522071820081-009f0129c71c?w=800",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    PublishedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            _context.Articles.AddRange(sampleArticles);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sample articles seeded successfully", count = sampleArticles.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding articles");
            return StatusCode(500, "An error occurred while seeding articles");
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
