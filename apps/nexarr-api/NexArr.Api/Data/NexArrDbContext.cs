using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace NexArr.Api.Data;

public sealed class NexArrDbContext(DbContextOptions<NexArrDbContext> options) : PlatformDbContext(options);
