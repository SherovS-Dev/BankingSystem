using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankingSystem.API.Interfaces;
using BankingSystem.API.Models;

namespace BankingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountRepository accountRepository, ILogger<AccountsController> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    /// <summary>
    /// Получить информацию о счете
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Account>> GetAccount(int id)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(id);
            
            if (account == null)
            {
                _logger.LogWarning("Account {AccountId} not found", id);
                return NotFound(new { message = "Счет не найден" });
            }

            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account {AccountId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить все счета клиента
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<Account>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Account>>> GetCustomerAccounts(int customerId)
    {
        try
        {
            var accounts = await _accountRepository.GetByCustomerIdAsync(customerId);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts for customer {CustomerId}", customerId);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }

    /// <summary>
    /// Получить баланс счета
    /// </summary>
    [HttpGet("{id}/balance")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetBalance(int id)
    {
        try
        {
            var balance = await _accountRepository.GetBalanceAsync(id);
            return Ok(new { accountId = id, balance });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance for account {AccountId}", id);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
        }
    }
}