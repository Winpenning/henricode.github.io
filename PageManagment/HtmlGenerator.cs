using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PageManagment;

public static class HtmlGenerator
{
    /// <summary>
    /// Recebe a lista de blocos parseados e gera o HTML completo do artigo.
    /// Retorna o HTML como string e o slug gerado para o nome do arquivo.
    /// </summary>
    public static (string Html, string Slug) GenerateHTML(List<ContentBlock> blocks)
    {
        // ── Extrai o título (sempre o primeiro bloco) ─────────
        var title = blocks
            .FirstOrDefault(b => b.Type == BlockType.Title)?.Value
            ?? "Sem Título";

        var slug = GenerateSlug(title);
        var data = DateTime.Now.ToString("MMM yyyy", new CultureInfo("pt-BR"));
        var ano  = DateTime.Now.Year;

        // ── Gera o conteúdo interno do artigo ─────────────────
        var conteudo = GenerateContent(blocks);

        // ── Monta o HTML final no template do blog ────────────
        var html = $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>{title} — Meu Blog</title>
              <link rel="stylesheet" href="../style.css" />
            </head>
            <body>
              <div class="container article-container">

                <nav class="back-nav">
                  <a href="../index.html" class="back-link">← Voltar ao blog</a>
                </nav>

                <article>

                  <header class="article-header">
                    <div class="article-meta">
                      <span class="article-date">{data}</span>
                    </div>
                    <h1 class="article-title">{title}</h1>
                  </header>

                  <div class="article-content">
            {conteudo}
                  </div>

                </article>

                <footer class="footer">
                  <p class="footer-text">© {ano} — Meu Blog</p>
                </footer>

              </div>
            </body>
            </html>
            """;

        return (html, slug);
    }

    // ── Gera as tags HTML de cada bloco de conteúdo ───────────────
    private static string GenerateContent(List<ContentBlock> blocks)
    {
        var sb = new StringBuilder();

        // Pula o título (índice 0), ele já foi usado no cabeçalho
        foreach (var bloco in blocks.Where(b => b.Type != BlockType.Title))
        {
            var linha = bloco.Type switch
            {
                BlockType.Subtitle => GenerateSubtitle(bloco.Value),
                BlockType.Text => GenerateText(bloco.Value),
                BlockType.Image    => GenerateImage(bloco.Value),
                _                   => string.Empty
            };

            sb.AppendLine(linha);
        }

        return sb.ToString();
    }

    // ── Builders de cada tag ──────────────────────────────────────

    private static string GenerateSubtitle(string texto) =>
        $"        <h2>{Escape(texto)}</h2>";

    private static string GenerateText(string texto) =>
        $"""
                <p>
                  {Escape(texto)}
                </p>
        """;

    private static string GenerateImage(string arquivo)
    {
        // Nome do arquivo sem extensão vira o alt text
        var alt = Path.GetFileNameWithoutExtension(arquivo);
        return $"        <img class=\"article-image\" src=\"images/{arquivo}\" alt=\"{Escape(alt)}\" />";
    }

    // ── Escapa caracteres especiais do HTML ───────────────────────
    private static string Escape(string texto) =>
        texto
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");

    // ── Gera slug a partir do título ──────────────────────────────
    // Ex: "Meu Primeiro Artigo!" → "meu-primeiro-artigo"
    public static string GenerateSlug(string titulo)
    {
        // Normaliza para remover acentos
        var normalizado = titulo
            .Normalize(NormalizationForm.FormD);

        var semAcento = new StringBuilder();
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                semAcento.Append(c);
        }

        return Regex.Replace(
                semAcento.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant(),
                @"[^a-z0-9\s-]", "")   // Remove caracteres especiais
            .Trim()
            .Replace(' ', '-')          // Espaços viram hífens
            .Replace("--", "-");        // Evita hífens duplos
    }
}