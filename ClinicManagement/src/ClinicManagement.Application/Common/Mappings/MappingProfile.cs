using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicManagement.Shared.Common;

namespace ClinicManagement.Application.Common.Mappings;

/// <summary>
/// A DTO implements this to declare "I map from entity T" right next to its
/// own property definitions, instead of a separate hand-maintained
/// CreateMap&lt;Entity, Dto&gt; buried in a central profile. MappingProfile
/// below discovers every IMapFrom&lt;T&gt; in the assembly automatically.
/// Override Mapping() only when the default flattening AutoMapper does
/// isn't enough (e.g. combining two source properties into one DTO field).
/// </summary>
public interface IMapFrom<T>
{
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(typeof(MappingProfile).Assembly);
    }

    private void ApplyMappingsFromAssembly(System.Reflection.Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);

        var mappingMethodName = nameof(IMapFrom<object>.Mapping);

        var types = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType))
            .ToList();

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetMethod(mappingMethodName) ?? type.GetInterface("IMapFrom`1")?.GetMethod(mappingMethodName);
            methodInfo?.Invoke(instance, new object[] { this });
        }
    }
}

public static class MappingExtensions
{
    /// <summary>
    /// Projects straight to the DTO shape in SQL (AutoMapper ProjectTo) and
    /// pages the result — avoids pulling full entities into memory just to
    /// discard most of their columns.
    /// </summary>
    public static Task<PaginatedList<TDto>> ToPaginatedListAsync<TDto>(
        this IQueryable<TDto> queryable, int pageNumber, int pageSize) =>
        PaginatedList<TDto>.CreateAsync(queryable, pageNumber, pageSize);

    public static IQueryable<TDto> ProjectToDto<TSource, TDto>(this IQueryable<TSource> queryable, IConfigurationProvider config) =>
        queryable.ProjectTo<TDto>(config);
}
