using Application.Dtos.ToDo.Response;
using Domain.Entities;
using Xunit; 

namespace Application.UnitTests.Tests.Mapper;

public class MapperTests
{
    [Fact]
    public void ToDoEntity_ToDoResponse_Mapper_AvoidMissingFields()
    {
        IReadOnlyList<string> ignoredFields =
        [
            nameof(ToDoEntity.Created), 
            nameof(ToDoEntity.CreatedBy),
            nameof(ToDoEntity.LastModified),
            nameof(ToDoEntity.LastModifiedBy)
        ];
        
        // Take all the entity properties
        var entityProperties = typeof(ToDoEntity).GetProperties()
            .Select(p => p.Name)
            .ToList();

        // Take all the dto properties
        var dtoProperties = typeof(ToDoResponse).GetProperties()
            .Select(p => p.Name)
            .ToList();

        // Check the differences (excluding fields which have been ignored on purpose)
        var missingInDto = entityProperties
            .Except(ignoredFields)
            .Except(dtoProperties)
            .ToList();

        // If there is a field in the entity but not in the DTO, the test will fail
        Assert.Empty(missingInDto); 
    }
}
