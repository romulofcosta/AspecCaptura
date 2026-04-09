using AspecCaptura.Models;

namespace Tests.Builders;

/// <summary>
/// Builder pattern for creating PatrimonioItem test data
/// </summary>
public class PatrimonioItemBuilder
{
    private readonly PatrimonioItem _item;

    public PatrimonioItemBuilder()
    {
        _item = new PatrimonioItem
        {
            IdPatomb = Random.Shared.NextInt64(100000, 999999),
            Nutomb = $"TST{Random.Shared.Next(1000, 9999)}",
            Esfera = "E",
            Deprod = "TESTE",
            Descricao = "Item de patrimônio para testes",
            Localizacao = "Local de teste",
            ValorEstimado = 1000.00m,
            Estado = ConservationState.Bom,
            LastRecognized = null,
            RecognitionSource = null,
            RecognitionConfidence = null
        };
    }

    public static PatrimonioItemBuilder Create() => new();

    public PatrimonioItemBuilder WithId(long id)
    {
        _item.IdPatomb = id;
        return this;
    }

    public PatrimonioItemBuilder WithNutomb(string nutomb)
    {
        _item.Nutomb = nutomb;
        return this;
    }

    public PatrimonioItemBuilder WithEsfera(string esfera)
    {
        _item.Esfera = esfera;
        return this;
    }

    public PatrimonioItemBuilder WithDeprod(string deprod)
    {
        _item.Deprod = deprod;
        return this;
    }

    public PatrimonioItemBuilder WithDescricao(string? descricao)
    {
        _item.Descricao = descricao;
        return this;
    }

    public PatrimonioItemBuilder WithLocalizacao(string? localizacao)
    {
        _item.Localizacao = localizacao;
        return this;
    }

    public PatrimonioItemBuilder WithValorEstimado(decimal? valor)
    {
        _item.ValorEstimado = valor;
        return this;
    }

    public PatrimonioItemBuilder WithEstado(ConservationState? estado)
    {
        _item.Estado = estado;
        return this;
    }

    public PatrimonioItemBuilder WithRecognitionData(RecognitionSource source, float confidence, DateTime? timestamp = null)
    {
        _item.RecognitionSource = source;
        _item.RecognitionConfidence = confidence;
        _item.LastRecognized = timestamp ?? DateTime.Now;
        return this;
    }

    public PatrimonioItemBuilder AsQRRecognized(float confidence = 0.95f)
    {
        return WithRecognitionData(RecognitionSource.QR, confidence);
    }

    public PatrimonioItemBuilder AsOCRRecognized(float confidence = 0.85f)
    {
        return WithRecognitionData(RecognitionSource.OCR, confidence);
    }

    public PatrimonioItemBuilder AsManualEntry()
    {
        return WithRecognitionData(RecognitionSource.Manual, 1.0f);
    }

    public PatrimonioItem Build() => _item;

    // Predefined scenarios by Esfera
    public static PatrimonioItemBuilder EsferaEstadual() => Create()
        .WithEsfera("E")
        .WithNutomb("E123456")
        .WithDescricao("Patrimônio Estadual")
        .WithDeprod("ESTADO");

    public static PatrimonioItemBuilder EsferaMunicipal() => Create()
        .WithEsfera("M")
        .WithNutomb("M789012")
        .WithDescricao("Patrimônio Municipal")
        .WithDeprod("MUNICIPIO");

    public static PatrimonioItemBuilder EsferaFederal() => Create()
        .WithEsfera("F")
        .WithNutomb("F345678")
        .WithDescricao("Patrimônio Federal")
        .WithDeprod("FEDERAL");

    public static PatrimonioItemBuilder EsferaAutarquia() => Create()
        .WithEsfera("A")
        .WithNutomb("A901234")
        .WithDescricao("Patrimônio de Autarquia")
        .WithDeprod("AUTARQUIA");

    // Predefined scenarios by conservation state
    public static PatrimonioItemBuilder InGoodCondition() => Create()
        .WithEstado(ConservationState.Bom)
        .WithDescricao("Item em bom estado de conservação");

    public static PatrimonioItemBuilder InRegularCondition() => Create()
        .WithEstado(ConservationState.Regular)
        .WithDescricao("Item em estado regular de conservação");

    public static PatrimonioItemBuilder InPessimoCondition() => Create()
        .WithEstado(ConservationState.Pessimo)
        .WithDescricao("Item em péssimo estado");

    public static PatrimonioItemBuilder InNewCondition() => Create()
        .WithEstado(ConservationState.Novo)
        .WithDescricao("Item novo");

    // Predefined scenarios by value range
    public static PatrimonioItemBuilder LowValue() => Create()
        .WithValorEstimado(Random.Shared.Next(100, 500))
        .WithDescricao("Item de baixo valor");

    public static PatrimonioItemBuilder MediumValue() => Create()
        .WithValorEstimado(Random.Shared.Next(500, 5000))
        .WithDescricao("Item de valor médio");

    public static PatrimonioItemBuilder HighValue() => Create()
        .WithValorEstimado(Random.Shared.Next(5000, 50000))
        .WithDescricao("Item de alto valor");

    // Common test scenarios
    public static PatrimonioItemBuilder ValidForTesting() => Create()
        .WithNutomb("TST001")
        .WithDescricao("Item válido para testes")
        .WithLocalizacao("Ambiente de teste")
        .WithValorEstimado(1000.00m)
        .WithEstado(ConservationState.Bom);

    public static PatrimonioItemBuilder WithSpecificCode(string code) => Create()
        .WithNutomb(code)
        .WithDescricao($"Item com código específico: {code}");

    public static PatrimonioItemBuilder[] CreateBatch(int count, string esfera = "E")
    {
        return Enumerable.Range(1, count)
            .Select(i => Create()
                .WithNutomb($"{esfera}{i:D6}")
                .WithEsfera(esfera)
                .WithDescricao($"Item de teste {i}")
                .WithValorEstimado(Random.Shared.Next(100, 10000)))
            .ToArray();
    }
}