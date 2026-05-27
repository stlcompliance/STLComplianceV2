using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Data;

public sealed class ComplianceCoreDbContext(DbContextOptions<ComplianceCoreDbContext> options)
    : PlatformDbContext(options);
