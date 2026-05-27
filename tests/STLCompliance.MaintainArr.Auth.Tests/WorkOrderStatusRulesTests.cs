using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class WorkOrderStatusRulesTests
{
    [Theory]
    [InlineData(WorkOrderStatuses.Open, WorkOrderStatuses.InProgress, true)]
    [InlineData(WorkOrderStatuses.Open, WorkOrderStatuses.Cancelled, true)]
    [InlineData(WorkOrderStatuses.InProgress, WorkOrderStatuses.Completed, true)]
    [InlineData(WorkOrderStatuses.InProgress, WorkOrderStatuses.Cancelled, true)]
    [InlineData(WorkOrderStatuses.Open, WorkOrderStatuses.Completed, false)]
    [InlineData(WorkOrderStatuses.Completed, WorkOrderStatuses.Open, false)]
    [InlineData(WorkOrderStatuses.Cancelled, WorkOrderStatuses.InProgress, false)]
    public void CanTransition_respects_lifecycle(string from, string to, bool expected) =>
        Assert.Equal(expected, WorkOrderStatusRules.CanTransition(from, to));
}
