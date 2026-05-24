namespace PageManagment;

// Tipos de bloco que o parser pode identificar no .txt
public enum BlockType
{
    Title,    // Primeira linha do arquivo
    Subtitle, // Linha única entre espaços em branco
    Text, // Bloco com múltiplas linhas
    Image     // Linha que começa com "img:"
}

// Representa um elemento de conteúdo já parseado
public record ContentBlock(BlockType Type, string Value);