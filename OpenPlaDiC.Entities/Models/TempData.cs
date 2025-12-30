using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class TempData
{
    public Guid Id { get; set; }

    public string? ExternalId { get; set; }

    public string? SecondaryId { get; set; }

    public Guid? RefId { get; set; }

    public string ProcessName { get; set; } = null!;

    public DateTime ProcessDate { get; set; }

    public int ProcessStage { get; set; }

    public int Sequence { get; set; }

    public bool IsActive { get; set; }

    public bool IsSelected { get; set; }

    public string? Name { get; set; }

    public string? Code { get; set; }

    public string? IX1 { get; set; }

    public string? IX2 { get; set; }

    public string? IX3 { get; set; }

    public int? I01 { get; set; }

    public int? I02 { get; set; }

    public int? I03 { get; set; }

    public int? I04 { get; set; }

    public int? I05 { get; set; }

    public int? N01 { get; set; }

    public int? N02 { get; set; }

    public int? N03 { get; set; }

    public int? N04 { get; set; }

    public int? N05 { get; set; }

    public string? T01 { get; set; }

    public string? T02 { get; set; }

    public string? T03 { get; set; }

    public string? T04 { get; set; }

    public string? T05 { get; set; }

    public string? T06 { get; set; }

    public string? T07 { get; set; }

    public string? T08 { get; set; }

    public string? T09 { get; set; }

    public string? T10 { get; set; }

    public DateTime? F01 { get; set; }

    public DateTime? F02 { get; set; }

    public DateTime? F03 { get; set; }

    public DateTime? F04 { get; set; }

    public DateTime? F05 { get; set; }

    public string? M01 { get; set; }

    public string? M02 { get; set; }

    public string? M03 { get; set; }

    public string? M04 { get; set; }

    public string? M05 { get; set; }

    public bool? B01 { get; set; }

    public bool? B02 { get; set; }

    public bool? B03 { get; set; }

    public bool? B04 { get; set; }

    public bool? B05 { get; set; }

    public string? D01 { get; set; }

    public string? D02 { get; set; }

    public string? D03 { get; set; }

    public string? D04 { get; set; }

    public string? D05 { get; set; }

    public string? D06 { get; set; }

    public string? D07 { get; set; }

    public string? D08 { get; set; }

    public string? D09 { get; set; }

    public string? D10 { get; set; }

    public string? D11 { get; set; }

    public string? D12 { get; set; }

    public string? D13 { get; set; }

    public string? D14 { get; set; }

    public string? D15 { get; set; }

    public string? D16 { get; set; }

    public string? D17 { get; set; }

    public string? D18 { get; set; }

    public string? D19 { get; set; }

    public string? D20 { get; set; }

    public string? D21 { get; set; }

    public string? D22 { get; set; }

    public string? D23 { get; set; }

    public string? D24 { get; set; }

    public string? D25 { get; set; }

    public string? D26 { get; set; }

    public string? D27 { get; set; }

    public string? D28 { get; set; }

    public string? D29 { get; set; }

    public string? D30 { get; set; }

    public string? D31 { get; set; }

    public string? D32 { get; set; }

    public string? D33 { get; set; }

    public string? D34 { get; set; }

    public string? D35 { get; set; }

    public string? D36 { get; set; }

    public string? D37 { get; set; }

    public string? D38 { get; set; }

    public string? D39 { get; set; }

    public string? D40 { get; set; }

    public string? D41 { get; set; }

    public string? D42 { get; set; }

    public string? D43 { get; set; }

    public string? D44 { get; set; }

    public string? D45 { get; set; }

    public string? D46 { get; set; }

    public string? D47 { get; set; }

    public string? D48 { get; set; }

    public string? D49 { get; set; }

    public string? D50 { get; set; }

    public string? D51 { get; set; }

    public string? D52 { get; set; }

    public string? D53 { get; set; }

    public string? D54 { get; set; }

    public string? D55 { get; set; }

    public string? D56 { get; set; }

    public string? D57 { get; set; }

    public string? D58 { get; set; }

    public string? D59 { get; set; }

    public string? D60 { get; set; }

    public string? D61 { get; set; }

    public string? D62 { get; set; }

    public string? D63 { get; set; }

    public string? D64 { get; set; }

    public string? D65 { get; set; }

    public string? D66 { get; set; }

    public string? D67 { get; set; }

    public string? D68 { get; set; }

    public string? D69 { get; set; }

    public string? D70 { get; set; }

    public string? D71 { get; set; }

    public string? D72 { get; set; }

    public string? D73 { get; set; }

    public string? D74 { get; set; }

    public string? D75 { get; set; }

    public string? D76 { get; set; }

    public string? D77 { get; set; }

    public string? D78 { get; set; }

    public string? D79 { get; set; }

    public string? D80 { get; set; }

    public string? D81 { get; set; }

    public string? D82 { get; set; }

    public string? D83 { get; set; }

    public string? D84 { get; set; }

    public string? D85 { get; set; }

    public string? D86 { get; set; }

    public string? D87 { get; set; }

    public string? D88 { get; set; }

    public string? D89 { get; set; }

    public string? D90 { get; set; }

    public string? D91 { get; set; }

    public string? D92 { get; set; }

    public string? D93 { get; set; }

    public string? D94 { get; set; }

    public string? D95 { get; set; }

    public string? D96 { get; set; }

    public string? D97 { get; set; }

    public string? D98 { get; set; }

    public string? D99 { get; set; }

    public string? D100 { get; set; }

    public string? D101 { get; set; }

    public string? D102 { get; set; }

    public string? D103 { get; set; }

    public string? D104 { get; set; }

    public string? D105 { get; set; }

    public string? D106 { get; set; }

    public string? D107 { get; set; }

    public string? D108 { get; set; }

    public string? D109 { get; set; }

    public string? D110 { get; set; }

    public string? D111 { get; set; }

    public string? D112 { get; set; }

    public string? D113 { get; set; }

    public string? D114 { get; set; }

    public string? D115 { get; set; }

    public string? D116 { get; set; }

    public string? D117 { get; set; }

    public string? D118 { get; set; }

    public string? D119 { get; set; }

    public string? D120 { get; set; }

    public string? D121 { get; set; }

    public string? D122 { get; set; }

    public string? D123 { get; set; }

    public string? D124 { get; set; }

    public string? D125 { get; set; }

    public string? D126 { get; set; }

    public string? D127 { get; set; }

    public string? D128 { get; set; }

    public string? D129 { get; set; }

    public string? D130 { get; set; }

    public string? D131 { get; set; }

    public string? D132 { get; set; }

    public string? D133 { get; set; }

    public string? D134 { get; set; }

    public string? D135 { get; set; }

    public string? D136 { get; set; }

    public string? D137 { get; set; }

    public string? D138 { get; set; }

    public string? D139 { get; set; }

    public string? D140 { get; set; }

    public string? D141 { get; set; }

    public string? D142 { get; set; }

    public string? D143 { get; set; }

    public string? D144 { get; set; }

    public string? D145 { get; set; }

    public string? D146 { get; set; }

    public string? D147 { get; set; }

    public string? D148 { get; set; }

    public string? D149 { get; set; }

    public string? D150 { get; set; }

    public string? D151 { get; set; }

    public string? D152 { get; set; }

    public string? D153 { get; set; }

    public string? D154 { get; set; }

    public string? D155 { get; set; }

    public string? D156 { get; set; }

    public string? D157 { get; set; }

    public string? D158 { get; set; }

    public string? D159 { get; set; }

    public string? D160 { get; set; }

    public string? D161 { get; set; }

    public string? D162 { get; set; }

    public string? D163 { get; set; }

    public string? D164 { get; set; }

    public string? D165 { get; set; }

    public string? D166 { get; set; }

    public string? D167 { get; set; }

    public string? D168 { get; set; }

    public string? D169 { get; set; }

    public string? D170 { get; set; }

    public string? D171 { get; set; }

    public string? D172 { get; set; }

    public string? D173 { get; set; }

    public string? D174 { get; set; }

    public string? D175 { get; set; }

    public string? D176 { get; set; }

    public string? D177 { get; set; }

    public string? D178 { get; set; }

    public string? D179 { get; set; }

    public string? D180 { get; set; }

    public string? D181 { get; set; }

    public string? D182 { get; set; }

    public string? D183 { get; set; }

    public string? D184 { get; set; }

    public string? D185 { get; set; }

    public string? D186 { get; set; }

    public string? D187 { get; set; }

    public string? D188 { get; set; }

    public string? D189 { get; set; }

    public string? D190 { get; set; }

    public string? D191 { get; set; }

    public string? D192 { get; set; }

    public string? D193 { get; set; }

    public string? D194 { get; set; }

    public string? D195 { get; set; }

    public string? D196 { get; set; }

    public string? D197 { get; set; }

    public string? D198 { get; set; }

    public string? D199 { get; set; }

    public string? D200 { get; set; }

    public string? D201 { get; set; }

    public string? D202 { get; set; }

    public string? D203 { get; set; }

    public string? D204 { get; set; }

    public string? D205 { get; set; }

    public string? D206 { get; set; }

    public string? D207 { get; set; }

    public string? D208 { get; set; }

    public string? D209 { get; set; }

    public string? D210 { get; set; }

    public string? D211 { get; set; }

    public string? D212 { get; set; }

    public string? D213 { get; set; }

    public string? D214 { get; set; }

    public string? D215 { get; set; }

    public string? D216 { get; set; }

    public string? D217 { get; set; }

    public string? D218 { get; set; }

    public string? D219 { get; set; }

    public string? D220 { get; set; }

    public DateTime CreationDate { get; set; }
}
