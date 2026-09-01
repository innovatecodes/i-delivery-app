using System.Collections.Generic;
using System.Linq;

namespace IDelivery.Domain.Common.ValueObjects;

// Classe abstrata base para todos os Value Objects do sistema
// Value Object = objeto imutável que se identifica por suas propriedades, não por um ID
public abstract class ValueObject
{
    // Método abstrato que cada Value Object concreto deve implementar
    // Retorna todas as propriedades que compõem a igualdade do objeto
    // Exemplo: em Address, retorna Street, Number, City, etc
    protected abstract IEnumerable<object?> GetEqualityComponents();

    // Sobrescreve o Equals do Object para comparar Value Objects por valor
    // Dois Value Objects são iguais se TODAS as suas propriedades forem iguais
    public override bool Equals(object? obj)
    {
        // 1. Verifica se o objeto é nulo
        // 2. Verifica se os tipos são exatamente iguais (não permite herança)
        if (obj is null || obj.GetType() != GetType()) return false;
        
        // Faz o cast para ValueObject
        var other = (ValueObject)obj;
        
        // Compara as listas de componentes de igualdade
        // SequenceEqual verifica se duas sequências têm os mesmos elementos na mesma ordem
        // Exemplo: Address(Street, Number, City) == Address(Street, Number, City)
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    // Sobrescreve o GetHashCode para ser consistente com o Equals
    // Dois objetos iguais DEVEM ter o mesmo HashCode
    public override int GetHashCode()
    {
        // Pega todos os componentes, calcula o hash de cada um
        // Se o componente for nulo, usa 0
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            // Combina todos os hashes usando XOR (^)
            // XOR é usado porque é comutativo e associativo
            .Aggregate((x, y) => x ^ y);
    }

    // Sobrescreve o operador == para que funcione com Value Objects
    // Exemplo: endereco1 == endereco2
    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        // Se ambos são nulos, são iguais (null == null)
        if (a is null && b is null) return true;
        
        // Se apenas um é nulo, são diferentes
        if (a is null || b is null) return false;
        
        // Usa o Equals que implementamos acima
        return a.Equals(b);
    }

    // Sobrescreve o operador != (diferente) como negação do ==
    // Exemplo: endereco1 != endereco2
    public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
}