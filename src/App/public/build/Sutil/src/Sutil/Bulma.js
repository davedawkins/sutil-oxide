import { HtmlEngine$1__i_BB573A, HtmlEngine$1__select_BB573A, HtmlEngine$1__div_BB573A } from "../../../fable_modules/Feliz.Engine.1.0.0-beta-004/HtmlEngine.fs.js";
import { class$0027 } from "./CoreElements.js";
import { singleton, append, delay, toList } from "../../../fable_modules/fable-library.3.7.20/Seq.js";
import { PseudoCss_addClass, EngineHelpers_Html, EngineHelpers_Attr, EngineHelpers_Css, SutilAttrEngine__style_68BDC580 } from "./Html.js";
import { CssEngine$1__padding_Z6BEC75E0, CssEngine$1__maxWidth_Z445F6BAF, CssEngine$1__padding_Z524259A4, CssEngine$1__height_Z445F6BAF } from "../../../fable_modules/Feliz.Engine.1.0.0-beta-004/CssEngine.fs.js";
import { ofArray, singleton as singleton_1, append as append_1 } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { AttrEngine$1__multiple_Z1FBCCD16 } from "../../../fable_modules/Feliz.Engine.1.0.0-beta-004/AttrEngine.fs.js";
import { withCustomRules, rule } from "./Styling.js";
import { int32ToString } from "../../../fable_modules/fable-library.3.7.20/Util.js";
import { BulmaModifiersEngine$1__get_column, BulmaModifiersEngine$1__get_columns, BulmaModifiersEngine$1__get_tile, BulmaModifiersEngine$1__get_hero, BulmaModifiersEngine$1__get_section, BulmaModifiersEngine$1__get_level, BulmaModifiersEngine$1__get_container, BulmaModifiersEngine$1__get_delete, BulmaModifiersEngine$1__get_content, BulmaModifiersEngine$1__get_ol, BulmaModifiersEngine$1__get_control, BulmaModifiersEngine$1__get_select, BulmaModifiersEngine$1__get_icon, BulmaModifiersEngine$1__get_field, BulmaModifiersEngine$1__get_textarea, BulmaModifiersEngine$1__get_fieldLabel, BulmaModifiersEngine$1__get_buttons, BulmaModifiersEngine$1__get_button, BulmaModifiersEngine$1__get_input, BulmaModifiersEngine$1__get_file, BulmaModifiersEngine$1__get_paginationLink, BulmaModifiersEngine$1__get_navbarItem, BulmaModifiersEngine$1__get_navbarLink, BulmaModifiersEngine$1__get_navbarDropdown, BulmaModifiersEngine$1__get_navbarBurger, BulmaModifiersEngine$1__get_navbarMenu, BulmaModifiersEngine$1__get_navbar, BulmaModifiersEngine$1__get_modalClose, BulmaModifiersEngine$1__get_modal, BulmaModifiersEngine$1__get_dropdown, BulmaModifiersEngine$1__get_cardHeaderTitle, BulmaModifiersEngine$1__get_breadcrumb, BulmaModifiersEngine$1__get_tab, BulmaModifiersEngine$1__get_tabs, BulmaModifiersEngine$1__get_title, BulmaModifiersEngine$1__get_tags, BulmaModifiersEngine$1__get_tag, BulmaModifiersEngine$1__get_tr, BulmaModifiersEngine$1__get_table, BulmaModifiersEngine$1__get_progress, BulmaModifiersEngine$1__get_image, BulmaModifiersEngine$1__get_color, BulmaModifiersEngine$1__get_text, BulmaModifiersEngine$1__get_spacing, BulmaModifiersEngine$1__get_size, BulmaEngine$1__get_m, BulmaModifiersEngine$1__get_helpers, BulmaEngine$1_$ctor_Z65B44AD2 } from "../../../fable_modules/Feliz.Engine.Bulma.1.0.0-beta-007/Bulma.fs.js";

export function Helpers_selectList(props) {
    return HtmlEngine$1__div_BB573A(EngineHelpers_Html, [class$0027("select is-multiple"), HtmlEngine$1__select_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton(SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, "auto"), CssEngine$1__padding_Z524259A4(EngineHelpers_Css, 0)])), delay(() => props)))))]);
}

export function Helpers_selectMultiple(props) {
    return HtmlEngine$1__div_BB573A(EngineHelpers_Html, [class$0027("select is-multiple"), HtmlEngine$1__select_BB573A(EngineHelpers_Html, append_1(singleton_1(AttrEngine$1__multiple_Z1FBCCD16(EngineHelpers_Attr, true)), props))]);
}

export const styleHelpers = ofArray([rule("h1", ofArray([PseudoCss_addClass("title"), PseudoCss_addClass("is-1")])), rule("h2", ofArray([PseudoCss_addClass("title"), PseudoCss_addClass("is-2")])), rule("h3", ofArray([PseudoCss_addClass("title"), PseudoCss_addClass("is-3")])), rule("h4", ofArray([PseudoCss_addClass("title"), PseudoCss_addClass("is-4")])), rule("h5", ofArray([PseudoCss_addClass("title"), PseudoCss_addClass("is-5")])), rule("button", singleton_1(PseudoCss_addClass("button"))), rule("input[type=\u0027file\u0027]", singleton_1(PseudoCss_addClass("file-cta"))), rule("input[type=\u0027text\u0027]", singleton_1(PseudoCss_addClass("input"))), rule("input[type=\u0027radio\u0027]", singleton_1(PseudoCss_addClass("radio"))), rule("input[type=\u0027checkbox\u0027]", singleton_1(PseudoCss_addClass("checkbox"))), rule("input[type=\u0027number\u0027]", ofArray([PseudoCss_addClass("input"), PseudoCss_addClass("is-small"), CssEngine$1__maxWidth_Z445F6BAF(EngineHelpers_Css, int32ToString(50) + "%")])), rule("input[type=\u0027range\u0027]", ofArray([PseudoCss_addClass("input"), PseudoCss_addClass("is-small"), CssEngine$1__maxWidth_Z445F6BAF(EngineHelpers_Css, int32ToString(50) + "%")])), rule(".is-multiple option", singleton_1(CssEngine$1__padding_Z6BEC75E0(EngineHelpers_Css, (0.5).toString() + "em", (1).toString() + "em")))]);

export function withBulmaHelpers(element) {
    return withCustomRules(styleHelpers, element);
}

export function FontAwesome_fa(name) {
    return HtmlEngine$1__i_BB573A(EngineHelpers_Html, [class$0027("fa fa-" + name)]);
}

export const bulma = BulmaEngine$1_$ctor_Z65B44AD2(EngineHelpers_Html, EngineHelpers_Attr);

export const helpers = BulmaModifiersEngine$1__get_helpers(BulmaEngine$1__get_m(bulma));

export const size = BulmaModifiersEngine$1__get_size(BulmaEngine$1__get_m(bulma));

export const spacing = BulmaModifiersEngine$1__get_spacing(BulmaEngine$1__get_m(bulma));

export const text = BulmaModifiersEngine$1__get_text(BulmaEngine$1__get_m(bulma));

export const color = BulmaModifiersEngine$1__get_color(BulmaEngine$1__get_m(bulma));

export const image = BulmaModifiersEngine$1__get_image(BulmaEngine$1__get_m(bulma));

export const progress = BulmaModifiersEngine$1__get_progress(BulmaEngine$1__get_m(bulma));

export const table = BulmaModifiersEngine$1__get_table(BulmaEngine$1__get_m(bulma));

export const tr = BulmaModifiersEngine$1__get_tr(BulmaEngine$1__get_m(bulma));

export const tag = BulmaModifiersEngine$1__get_tag(BulmaEngine$1__get_m(bulma));

export const tags = BulmaModifiersEngine$1__get_tags(BulmaEngine$1__get_m(bulma));

export const title = BulmaModifiersEngine$1__get_title(BulmaEngine$1__get_m(bulma));

export const tabs = BulmaModifiersEngine$1__get_tabs(BulmaEngine$1__get_m(bulma));

export const tab = BulmaModifiersEngine$1__get_tab(BulmaEngine$1__get_m(bulma));

export const breadcrumb = BulmaModifiersEngine$1__get_breadcrumb(BulmaEngine$1__get_m(bulma));

export const cardHeaderTitle = BulmaModifiersEngine$1__get_cardHeaderTitle(BulmaEngine$1__get_m(bulma));

export const dropdown = BulmaModifiersEngine$1__get_dropdown(BulmaEngine$1__get_m(bulma));

export const modal = BulmaModifiersEngine$1__get_modal(BulmaEngine$1__get_m(bulma));

export const modalClose = BulmaModifiersEngine$1__get_modalClose(BulmaEngine$1__get_m(bulma));

export const navbar = BulmaModifiersEngine$1__get_navbar(BulmaEngine$1__get_m(bulma));

export const navbarMenu = BulmaModifiersEngine$1__get_navbarMenu(BulmaEngine$1__get_m(bulma));

export const navbarBurger = BulmaModifiersEngine$1__get_navbarBurger(BulmaEngine$1__get_m(bulma));

export const navbarDropdown = BulmaModifiersEngine$1__get_navbarDropdown(BulmaEngine$1__get_m(bulma));

export const navbarLink = BulmaModifiersEngine$1__get_navbarLink(BulmaEngine$1__get_m(bulma));

export const navbarItem = BulmaModifiersEngine$1__get_navbarItem(BulmaEngine$1__get_m(bulma));

export const paginationLink = BulmaModifiersEngine$1__get_paginationLink(BulmaEngine$1__get_m(bulma));

export const file = BulmaModifiersEngine$1__get_file(BulmaEngine$1__get_m(bulma));

export const input = BulmaModifiersEngine$1__get_input(BulmaEngine$1__get_m(bulma));

export const button = BulmaModifiersEngine$1__get_button(BulmaEngine$1__get_m(bulma));

export const buttons = BulmaModifiersEngine$1__get_buttons(BulmaEngine$1__get_m(bulma));

export const fieldLabel = BulmaModifiersEngine$1__get_fieldLabel(BulmaEngine$1__get_m(bulma));

export const textarea = BulmaModifiersEngine$1__get_textarea(BulmaEngine$1__get_m(bulma));

export const field = BulmaModifiersEngine$1__get_field(BulmaEngine$1__get_m(bulma));

export const icon = BulmaModifiersEngine$1__get_icon(BulmaEngine$1__get_m(bulma));

export const select = BulmaModifiersEngine$1__get_select(BulmaEngine$1__get_m(bulma));

export const control = BulmaModifiersEngine$1__get_control(BulmaEngine$1__get_m(bulma));

export const ol = BulmaModifiersEngine$1__get_ol(BulmaEngine$1__get_m(bulma));

export const content = BulmaModifiersEngine$1__get_content(BulmaEngine$1__get_m(bulma));

export const delete$ = BulmaModifiersEngine$1__get_delete(BulmaEngine$1__get_m(bulma));

export const container = BulmaModifiersEngine$1__get_container(BulmaEngine$1__get_m(bulma));

export const level = BulmaModifiersEngine$1__get_level(BulmaEngine$1__get_m(bulma));

export const section = BulmaModifiersEngine$1__get_section(BulmaEngine$1__get_m(bulma));

export const hero = BulmaModifiersEngine$1__get_hero(BulmaEngine$1__get_m(bulma));

export const tile = BulmaModifiersEngine$1__get_tile(BulmaEngine$1__get_m(bulma));

export const columns = BulmaModifiersEngine$1__get_columns(BulmaEngine$1__get_m(bulma));

export const column = BulmaModifiersEngine$1__get_column(BulmaEngine$1__get_m(bulma));

