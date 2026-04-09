using AspecCaptura.Models;

namespace Tests.Builders;

/// <summary>
/// Builder pattern for creating Usuario test data
/// </summary>
public class UserBuilder
{
    private readonly Usuario _user;

    public UserBuilder()
    {
        _user = new Usuario
        {
            UsuarioNome = "test.user",
            NomeCompleto = "Usuário de Teste",
            Prefixo = "TST",
            Esfera = "E",
            Token = "mock-jwt-token-for-testing",
            Orgaos = new List<Orgao>(),
            Patrimonio = new List<PatrimonioItem>()
        };
    }

    public static UserBuilder Create() => new();

    public UserBuilder WithUsuarioNome(string usuarioNome)
    {
        _user.UsuarioNome = usuarioNome;
        return this;
    }

    public UserBuilder WithNomeCompleto(string nomeCompleto)
    {
        _user.NomeCompleto = nomeCompleto;
        return this;
    }

    public UserBuilder WithPrefixo(string prefixo)
    {
        _user.Prefixo = prefixo;
        return this;
    }

    public UserBuilder WithEsfera(string esfera)
    {
        _user.Esfera = esfera;
        return this;
    }

    public UserBuilder WithToken(string token)
    {
        _user.Token = token;
        return this;
    }

    public UserBuilder WithOrgao(Orgao orgao)
    {
        _user.Orgaos.Add(orgao);
        return this;
    }

    public UserBuilder WithOrgaos(params Orgao[] orgaos)
    {
        _user.Orgaos.AddRange(orgaos);
        return this;
    }

    public UserBuilder WithPatrimonio(PatrimonioItem item)
    {
        _user.Patrimonio.Add(item);
        return this;
    }

    public UserBuilder WithPatrimonios(params PatrimonioItem[] items)
    {
        _user.Patrimonio.AddRange(items);
        return this;
    }

    public Usuario Build() => _user;

    // Predefined scenarios
    public static UserBuilder EstadualUser() => Create()
        .WithUsuarioNome("user.estadual")
        .WithNomeCompleto("Usuário Estadual")
        .WithEsfera("E")
        .WithPrefixo("EST");

    public static UserBuilder MunicipalUser() => Create()
        .WithUsuarioNome("user.municipal")
        .WithNomeCompleto("Usuário Municipal")
        .WithEsfera("M")
        .WithPrefixo("MUN");

    public static UserBuilder FederalUser() => Create()
        .WithUsuarioNome("user.federal")
        .WithNomeCompleto("Usuário Federal")
        .WithEsfera("F")
        .WithPrefixo("FED");

    public static UserBuilder AutarquiaUser() => Create()
        .WithUsuarioNome("user.autarquia")
        .WithNomeCompleto("Usuário de Autarquia")
        .WithEsfera("A")
        .WithPrefixo("AUT");
}

/// <summary>
/// Builder pattern for creating Orgao test data
/// </summary>
public class OrgaoBuilder
{
    private readonly Orgao _orgao;

    public OrgaoBuilder()
    {
        _orgao = new Orgao
        {
            IdOrgao = Guid.NewGuid().ToString(),
            NomeOrgao = "Órgão de Teste",
            UnidadesOrcamentarias = new List<UnidadeOrcamentaria>()
        };
    }

    public static OrgaoBuilder Create() => new();

    public OrgaoBuilder WithId(string id)
    {
        _orgao.IdOrgao = id;
        return this;
    }

    public OrgaoBuilder WithNome(string nome)
    {
        _orgao.NomeOrgao = nome;
        return this;
    }

    public OrgaoBuilder WithUnidadeOrcamentaria(UnidadeOrcamentaria uo)
    {
        _orgao.UnidadesOrcamentarias.Add(uo);
        return this;
    }

    public OrgaoBuilder WithUnidadesOrcamentarias(params UnidadeOrcamentaria[] uos)
    {
        _orgao.UnidadesOrcamentarias.AddRange(uos);
        return this;
    }

    public Orgao Build() => _orgao;

    public static OrgaoBuilder SecretariaEducacao() => Create()
        .WithId("ORG001")
        .WithNome("Secretaria de Educação");

    public static OrgaoBuilder SecretariaSaude() => Create()
        .WithId("ORG002")
        .WithNome("Secretaria de Saúde");
}

/// <summary>
/// Builder pattern for creating UnidadeOrcamentaria test data
/// </summary>
public class UnidadeOrcamentariaBuilder
{
    private readonly UnidadeOrcamentaria _uo;

    public UnidadeOrcamentariaBuilder()
    {
        _uo = new UnidadeOrcamentaria
        {
            IdUO = Guid.NewGuid().ToString(),
            NomeUO = "Unidade Orçamentária de Teste",
            Areas = new List<Area>()
        };
    }

    public static UnidadeOrcamentariaBuilder Create() => new();

    public UnidadeOrcamentariaBuilder WithId(string id)
    {
        _uo.IdUO = id;
        return this;
    }

    public UnidadeOrcamentariaBuilder WithNome(string nome)
    {
        _uo.NomeUO = nome;
        return this;
    }

    public UnidadeOrcamentariaBuilder WithArea(Area area)
    {
        _uo.Areas.Add(area);
        return this;
    }

    public UnidadeOrcamentariaBuilder WithAreas(params Area[] areas)
    {
        _uo.Areas.AddRange(areas);
        return this;
    }

    public UnidadeOrcamentaria Build() => _uo;

    public static UnidadeOrcamentariaBuilder EscolaEstadual() => Create()
        .WithId("UO001")
        .WithNome("Escola Estadual Teste");

    public static UnidadeOrcamentariaBuilder HospitalPublico() => Create()
        .WithId("UO002")
        .WithNome("Hospital Público Teste");
}

/// <summary>
/// Builder pattern for creating Area test data
/// </summary>
public class AreaBuilder
{
    private readonly Area _area;

    public AreaBuilder()
    {
        _area = new Area
        {
            IdArea = Guid.NewGuid().ToString(),
            NomeArea = "Área de Teste",
            Subareas = new List<Subarea>()
        };
    }

    public static AreaBuilder Create() => new();

    public AreaBuilder WithId(string id)
    {
        _area.IdArea = id;
        return this;
    }

    public AreaBuilder WithNome(string nome)
    {
        _area.NomeArea = nome;
        return this;
    }

    public AreaBuilder WithSubarea(Subarea subarea)
    {
        _area.Subareas.Add(subarea);
        return this;
    }

    public AreaBuilder WithSubareas(params Subarea[] subareas)
    {
        _area.Subareas.AddRange(subareas);
        return this;
    }

    public Area Build() => _area;

    public static AreaBuilder Administracao() => Create()
        .WithId("AREA001")
        .WithNome("Administração");

    public static AreaBuilder TecnologiaInformacao() => Create()
        .WithId("AREA002")
        .WithNome("Tecnologia da Informação");
}

/// <summary>
/// Builder pattern for creating Subarea test data
/// </summary>
public class SubareaBuilder
{
    private readonly Subarea _subarea;

    public SubareaBuilder()
    {
        _subarea = new Subarea
        {
            IdSubarea = Guid.NewGuid().ToString(),
            NomeSubarea = "Subárea de Teste"
        };
    }

    public static SubareaBuilder Create() => new();

    public SubareaBuilder WithId(string id)
    {
        _subarea.IdSubarea = id;
        return this;
    }

    public SubareaBuilder WithNome(string nome)
    {
        _subarea.NomeSubarea = nome;
        return this;
    }

    public Subarea Build() => _subarea;

    public static SubareaBuilder RecursosHumanos() => Create()
        .WithId("SUB001")
        .WithNome("Recursos Humanos");

    public static SubareaBuilder Infraestrutura() => Create()
        .WithId("SUB002")
        .WithNome("Infraestrutura");
}