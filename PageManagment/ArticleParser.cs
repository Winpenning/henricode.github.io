namespace PageManagment;

public static class ArticleParser
{
    /// <summary>
    /// Lê um arquivo .txt e transforma em uma lista de BlocoConteudo.
    ///
    /// Regras do formato:
    ///   - Linha 0           → Título (h1)
    ///   - Linha em branco   → Separador de blocos
    ///   - Bloco 1 linha     → Subtítulo (h2), exceto se começar com "img:"
    ///   - Bloco N linhas    → Parágrafo (p); linhas "img:" viram imagem dentro do bloco
    ///   - img: arquivo.png  → Imagem (em qualquer posição após o título)
    /// </summary>
    public static async Task<List<ContentBlock>> ParseAsync(string articlePath)
    {
        if (!articlePath.Contains(".txt"))
            articlePath += ".txt";
        
        string[] lines = await File.ReadAllLinesAsync($"TxtArticles/{articlePath}");

        if (lines.Length == 0)
            throw new InvalidOperationException("O arquivo está vazio.");

        var blocks = new List<ContentBlock>();

        // ── Primeira linha sempre é o título ─────────────────
        blocks.Add(new ContentBlock(BlockType.Title, lines[0].Trim()));

        // ── Agrupa o restante em blocos separados por linha vazia ──
        var group = new List<string>();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                // Linha em branco: processa o grupo acumulado
                if (group.Count > 0)
                {
                    ProcessGroup(group, blocks);
                    group.Clear();
                }
            }
            else
            {
                group.Add(line.Trim());
            }
        }

        // Processa o último grupo (caso o arquivo não termine com linha em branco)
        if (group.Count > 0)
            ProcessGroup(group, blocks);

        return blocks;
    }

    // ── Lógica de classificação de cada grupo ─────────────────────
    private static void ProcessGroup(List<string> group, List<ContentBlock> blocks)
    {
        // Grupo de UMA linha → subtítulo ou imagem
        if (group.Count == 1)
        {
            var line = group[0];

            if (IsImage(line))
                blocks.Add(new ContentBlock(BlockType.Image, ExtractImagePath(line)));
            else
                blocks.Add(new ContentBlock(BlockType.Subtitle, line));

            return;
        }

        // Grupo de MÚLTIPLAS linhas → parágrafo(s), com possíveis imagens intercaladas
        var textLines = new List<string>();

        foreach (var line in group)
        {
            if (IsImage(line))
            {
                // Despeja o parágrafo acumulado antes da imagem
                if (textLines.Count > 0)
                {
                    blocks.Add(new ContentBlock(BlockType.Text, string.Join(" ", textLines)));
                    textLines.Clear();
                }

                blocks.Add(new ContentBlock(BlockType.Image, ExtractImagePath(line)));
            }
            else
            {
                textLines.Add(line);
            }
        }

        // Despeja o que sobrou como parágrafo
        if (textLines.Count > 0)
            blocks.Add(new ContentBlock(BlockType.Text, string.Join(" ", textLines)));
    }

    // ── Helpers ───────────────────────────────────────────────────
    private static bool IsImage(string line) =>
        line.StartsWith("img:", StringComparison.OrdinalIgnoreCase);

    private static string ExtractImagePath(string line) =>
        line[4..].Trim(); // Remove "img:" e espaços
}