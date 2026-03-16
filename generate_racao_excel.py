from __future__ import annotations

from datetime import datetime, timezone
from decimal import Decimal, ROUND_HALF_UP
from pathlib import Path
from xml.sax.saxutils import escape
import zipfile


OUTPUT_PATH = Path("precos_racao_golden_27kg.xlsx")


rows = [
    {
        "produto": "Racao Premier Pet Racas Especificas Golden Retriever Adulto",
        "peso_kg": Decimal("12"),
        "loja": "Petlove",
        "preco_anunciado": Decimal("264.90"),
        "preco_programado": Decimal("238.41"),
        "tipo_preco": "Preco cheio e recorrencia",
        "link": "https://www.petlove.com.br/racao-premier-pet-golden-retriever-adulto-especifica-para-goldens---12kg-31023724/p",
        "fonte": "Petlove, busca indexada em 2026-03-14",
    },
    {
        "produto": "Racao Premier Racas Especificas Golden Retriever para Caes Adultos",
        "peso_kg": Decimal("12"),
        "loja": "Cobasi",
        "preco_anunciado": Decimal("264.90"),
        "preco_programado": Decimal("238.41"),
        "tipo_preco": "Preco cheio e compra programada",
        "link": "https://www.cobasi.com.br/racao-premier-golden-retriever-adultos-3640999/p",
        "fonte": "Cobasi, busca indexada em 2026-03-14",
    },
    {
        "produto": "Racao Premier Racas Especificas Golden Retriever para Caes Adultos",
        "peso_kg": Decimal("12"),
        "loja": "Petz",
        "preco_anunciado": None,
        "preco_programado": Decimal("251.99"),
        "tipo_preco": "Preco para assinantes",
        "link": "https://www.petz.com.br/produto/racao-premier-racas-especificas-golden-retriever-para-caes-adultos-12kg",
        "fonte": "Petz, busca indexada em 2026-03-14",
    },
    {
        "produto": "Racao Royal Canin para Caes Adultos da Raca Golden Retriever",
        "peso_kg": Decimal("12"),
        "loja": "Petz",
        "preco_anunciado": None,
        "preco_programado": Decimal("395.99"),
        "tipo_preco": "Preco para assinantes",
        "link": "https://www.petz.com.br/produto/racao-royal-canin-para-caes-adultos-da-raca-golden-retriever-12kg",
        "fonte": "Petz, busca indexada em 2026-03-14",
    },
    {
        "produto": "Racao Royal Canin para Caes Adultos da Raca Golden Retriever",
        "peso_kg": Decimal("10.1"),
        "loja": "Petlove",
        "preco_anunciado": Decimal("370.99"),
        "preco_programado": Decimal("333.89"),
        "tipo_preco": "Preco cheio e recorrencia",
        "link": "https://www.petlove.com.br/racao-royal-canin-para-caes-adultos-da-raca-golden-retriever/p",
        "fonte": "Petlove, busca indexada em 2026-03-14",
    },
    {
        "produto": "Racao Golden Special para Caes Adultos Frango e Carne",
        "peso_kg": Decimal("15"),
        "loja": "Petlove",
        "preco_anunciado": Decimal("159.90"),
        "preco_programado": Decimal("143.91"),
        "tipo_preco": "Preco cheio e recorrencia",
        "link": "https://www.petlove.com.br/racao-golden-special-adulto-1014070/p",
        "fonte": "Petlove, busca indexada em 2026-03-14",
    },
    {
        "produto": "Racao Golden Special para Caes Adultos Frango e Carne",
        "peso_kg": Decimal("15"),
        "loja": "Cobasi",
        "preco_anunciado": Decimal("159.90"),
        "preco_programado": Decimal("143.91"),
        "tipo_preco": "Preco cheio e compra programada",
        "link": "https://www.cobasi.com.br/racao-golden-special-para-caes-adultos-frango-e-carne-3310549/p",
        "fonte": "Cobasi, busca indexada em 2026-03-14",
    },
    {
        "produto": "Racao Golden Formula para Caes Adultos Sabor Frango e Arroz",
        "peso_kg": Decimal("15"),
        "loja": "Petz",
        "preco_anunciado": Decimal("174.90"),
        "preco_programado": Decimal("157.41"),
        "tipo_preco": "Preco cheio e assinantes",
        "link": "https://www.petz.com.br/produto/racao-golden-formula-para-caes-adultos-sabor-frango-e-arroz-71457",
        "fonte": "Petz, busca indexada em 2026-03-14",
    },
]


def round_money(value: Decimal | None) -> Decimal | None:
    if value is None:
        return None
    return value.quantize(Decimal("0.01"), rounding=ROUND_HALF_UP)


for row in rows:
    base = row["preco_anunciado"] if row["preco_anunciado"] is not None else row["preco_programado"]
    row["preco_usado"] = base
    row["preco_por_kg"] = round_money(base / row["peso_kg"]) if base is not None else None


rows.sort(key=lambda item: (item["preco_por_kg"] is None, item["preco_por_kg"] or Decimal("999999")))


def col_name(index: int) -> str:
    name = ""
    while index > 0:
        index, rem = divmod(index - 1, 26)
        name = chr(65 + rem) + name
    return name


def inline_str_cell(ref: str, value: str) -> str:
    safe = escape(value)
    return f'<c r="{ref}" t="inlineStr"><is><t>{safe}</t></is></c>'


def number_cell(ref: str, value: Decimal | float | int) -> str:
    return f'<c r="{ref}"><v>{value}</v></c>'


def build_sheet_xml(sheet_rows: list[list[object]]) -> str:
    xml_rows = []
    for row_idx, row in enumerate(sheet_rows, start=1):
        cells = []
        for col_idx, value in enumerate(row, start=1):
            ref = f"{col_name(col_idx)}{row_idx}"
            if value is None:
                continue
            if isinstance(value, (int, float, Decimal)):
                cells.append(number_cell(ref, value))
            else:
                cells.append(inline_str_cell(ref, str(value)))
        xml_rows.append(f'<row r="{row_idx}">{"".join(cells)}</row>')
    return (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
        '<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">'
        '<sheetViews><sheetView workbookViewId="0"/></sheetViews>'
        '<sheetFormatPr defaultRowHeight="15"/>'
        '<cols>'
        '<col min="1" max="1" width="42" customWidth="1"/>'
        '<col min="2" max="2" width="10" customWidth="1"/>'
        '<col min="3" max="3" width="14" customWidth="1"/>'
        '<col min="4" max="7" width="16" customWidth="1"/>'
        '<col min="8" max="8" width="28" customWidth="1"/>'
        '<col min="9" max="9" width="85" customWidth="1"/>'
        '<col min="10" max="10" width="28" customWidth="1"/>'
        '</cols>'
        f'<sheetData>{"".join(xml_rows)}</sheetData>'
        '</worksheet>'
    )


header = [
    "Produto",
    "Peso (kg)",
    "Loja",
    "Preco anunciado (R$)",
    "Preco assinatura/programado (R$)",
    "Preco usado p/ comparacao (R$)",
    "Preco por kg (R$)",
    "Tipo de preco",
    "Link",
    "Data/Fonte",
]

data_sheet_rows: list[list[object]] = [header]
for row in rows:
    data_sheet_rows.append(
        [
            row["produto"],
            row["peso_kg"],
            row["loja"],
            row["preco_anunciado"],
            row["preco_programado"],
            row["preco_usado"],
            row["preco_por_kg"],
            row["tipo_preco"],
            row["link"],
            row["fonte"],
        ]
    )

summary_rows = [
    ["Resumo", "Valor"],
    ["Data de geracao", datetime.now(timezone.utc).astimezone().strftime("%Y-%m-%d %H:%M:%S %z")],
    ["Itens comparados", len(rows)],
    ["Menor preco por kg encontrado (R$)", rows[0]["preco_por_kg"]],
    ["Produto mais barato por kg", rows[0]["produto"]],
    ["Loja", rows[0]["loja"]],
]


CONTENT_TYPES = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/worksheets/sheet2.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>
  <Override PartName="/docProps/app.xml" ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml"/>
  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
</Types>
"""


ROOT_RELS = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="docProps/core.xml"/>
  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties" Target="docProps/app.xml"/>
</Relationships>
"""


WORKBOOK = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
 xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets>
    <sheet name="Precos" sheetId="1" r:id="rId1"/>
    <sheet name="Resumo" sheetId="2" r:id="rId2"/>
  </sheets>
</workbook>
"""


WORKBOOK_RELS = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet2.xml"/>
  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
</Relationships>
"""


STYLES = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <fonts count="1"><font><sz val="11"/><name val="Calibri"/></font></fonts>
  <fills count="2">
    <fill><patternFill patternType="none"/></fill>
    <fill><patternFill patternType="gray125"/></fill>
  </fills>
  <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
  <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
  <cellXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/></cellXfs>
  <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
</styleSheet>
"""


timestamp = datetime.now(timezone.utc).replace(microsecond=0).isoformat()
CORE = f"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties"
 xmlns:dc="http://purl.org/dc/elements/1.1/"
 xmlns:dcterms="http://purl.org/dc/terms/"
 xmlns:dcmitype="http://purl.org/dc/dcmitype/"
 xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <dc:creator>Codex</dc:creator>
  <cp:lastModifiedBy>Codex</cp:lastModifiedBy>
  <dcterms:created xsi:type="dcterms:W3CDTF">{timestamp}</dcterms:created>
  <dcterms:modified xsi:type="dcterms:W3CDTF">{timestamp}</dcterms:modified>
</cp:coreProperties>
"""


APP = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties"
 xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes">
  <Application>Microsoft Excel</Application>
</Properties>
"""


with zipfile.ZipFile(OUTPUT_PATH, "w", compression=zipfile.ZIP_DEFLATED) as workbook:
    workbook.writestr("[Content_Types].xml", CONTENT_TYPES)
    workbook.writestr("_rels/.rels", ROOT_RELS)
    workbook.writestr("xl/workbook.xml", WORKBOOK)
    workbook.writestr("xl/_rels/workbook.xml.rels", WORKBOOK_RELS)
    workbook.writestr("xl/styles.xml", STYLES)
    workbook.writestr("xl/worksheets/sheet1.xml", build_sheet_xml(data_sheet_rows))
    workbook.writestr("xl/worksheets/sheet2.xml", build_sheet_xml(summary_rows))
    workbook.writestr("docProps/core.xml", CORE)
    workbook.writestr("docProps/app.xml", APP)


print(f"Arquivo gerado: {OUTPUT_PATH.resolve()}")
