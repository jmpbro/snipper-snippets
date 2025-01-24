using Microsoft.AspNetCore.Mvc;
using System;
using SnipperSnippets.Models;

[ApiController]
[Route("api/[controller]")]
public class SnippetController : ControllerBase
{
    private readonly ISnippetRepository _snippetRepository;

    public SnippetController(ISnippetRepository snippetRepository)
    {
        _snippetRepository = snippetRepository;
    }

    [HttpPost]
    public IActionResult CreateSnippet([FromBody] SnippetRequest request)
    {
        // Validate the incoming request
        if (request == null || string.IsNullOrWhiteSpace(request.SnippetText) || string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Invalid request. Title and SnippetText are required.");
        }

        try
        {
            // Encrypt the snippet text
            string encryptedSnippet = EncryptionHelper.EncryptString(request.SnippetText);

            // Create and save the snippet
            var snippet = new Snippet
            {
                Id = 0, // Assuming 0 or a default value for new snippets
                Title = request.Title,
                SnippetText = encryptedSnippet
            };

            _snippetRepository.Save(snippet);

            // Return a successful response
            return CreatedAtAction(nameof(GetSnippet), new { id = snippet.Id }, new { snippet.Id });
        }
        catch (Exception ex)
        {
            // Log the exception (logging mechanism assumed)
            Console.Error.WriteLine(ex.Message);

            // Return an internal server error response
            return StatusCode(500, "An error occurred while creating the snippet.");
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetSnippet(int id)
    {
        try
        {
            // Fetch the snippet by ID
            var snippet = _snippetRepository.GetById(id);

            if (snippet == null)
            {
                return NotFound($"Snippet with ID {id} not found.");
            }

            // Decrypt the snippet text
            string decryptedSnippet = EncryptionHelper.DecryptString(snippet.SnippetText);

            // Return the snippet details
            return Ok(new
            {
                snippet.Title,
                SnippetText = decryptedSnippet
            });
        }
        catch (Exception ex)
        {
            // Log the exception (logging mechanism assumed)
            Console.Error.WriteLine(ex.Message);

            // Return an internal server error response
            return StatusCode(500, "An error occurred while fetching the snippet.");
        }
    }
}
