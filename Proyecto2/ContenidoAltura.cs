public class ContenidoAltura
{
    public int ValorAltura { get; set; }
    public string Letra { get; set; }

    public ContenidoAltura(int altura, string letra)
    {
        ValorAltura = altura;
        Letra = letra;
    }
}