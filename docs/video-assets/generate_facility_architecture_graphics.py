from pathlib import Path
from PIL import Image, ImageDraw, ImageFont


W, H = 1920, 1080
OUT = Path(__file__).resolve().parent / "facility-architecture"

BG = (18, 22, 30)
PANEL = (31, 38, 52)
PANEL_2 = (24, 30, 42)
TEXT = (238, 242, 248)
MUTED = (158, 170, 190)
BLUE = (64, 156, 255)
CYAN = (42, 209, 226)
GREEN = (80, 210, 136)
PURPLE = (160, 116, 255)
ORANGE = (255, 178, 72)
RED = (255, 92, 106)
BORDER = (74, 88, 112)


def font(size, bold=False):
    candidates = [
        r"C:\Windows\Fonts\msyhbd.ttc" if bold else r"C:\Windows\Fonts\msyh.ttc",
        r"C:\Windows\Fonts\simhei.ttf",
        r"C:\Windows\Fonts\arialbd.ttf" if bold else r"C:\Windows\Fonts\arial.ttf",
    ]
    for c in candidates:
        if Path(c).exists():
            return ImageFont.truetype(c, size)
    return ImageFont.load_default()


F_TITLE = font(64, True)
F_SUB = font(32)
F_HEAD = font(34, True)
F_BODY = font(27)
F_SMALL = font(23)
F_CODE = font(25)


def canvas():
    im = Image.new("RGB", (W, H), BG)
    d = ImageDraw.Draw(im)
    return im, d


def text(d, xy, s, fill=TEXT, f=F_BODY, anchor=None, align="left"):
    d.text(xy, s, font=f, fill=fill, anchor=anchor, align=align)


def box(d, xy, fill=PANEL, outline=BORDER, width=2, r=20):
    d.rounded_rectangle(xy, radius=r, fill=fill, outline=outline, width=width)


def line(d, a, b, fill=CYAN, width=5, arrow=True):
    d.line([a, b], fill=fill, width=width)
    if arrow:
        import math
        ang = math.atan2(b[1] - a[1], b[0] - a[0])
        size = 18
        p1 = (b[0] - size * math.cos(ang - 0.45), b[1] - size * math.sin(ang - 0.45))
        p2 = (b[0] - size * math.cos(ang + 0.45), b[1] - size * math.sin(ang + 0.45))
        d.polygon([b, p1, p2], fill=fill)


def pill(d, xy, label, fill, fg=(14, 18, 26)):
    x1, y1, x2, y2 = xy
    d.rounded_rectangle(xy, radius=18, fill=fill)
    text(d, ((x1 + x2) / 2, (y1 + y2) / 2), label, fill=fg, f=F_SMALL, anchor="mm")


def header(d, title, subtitle):
    text(d, (90, 70), title, f=F_TITLE)
    text(d, (92, 150), subtitle, fill=MUTED, f=F_SUB)


def save(im, name):
    OUT.mkdir(parents=True, exist_ok=True)
    im.save(OUT / name, quality=95)


def draw_board(d, x, y, size=500):
    cells = 9
    step = size // cells
    for i in range(cells):
        for j in range(cells):
            if i in (0, cells - 1) or j in (0, cells - 1):
                cx, cy = x + i * step, y + j * step
                fill = PANEL_2
                if (i + j) % 5 == 0:
                    fill = (44, 67, 85)
                d.rounded_rectangle((cx, cy, cx + step - 5, cy + step - 5), radius=10, fill=fill, outline=BORDER)
    d.ellipse((x + step * 2 + 8, y + step * 7 + 8, x + step * 2 + 46, y + step * 7 + 46), fill=ORANGE)


def slide_1():
    im, d = canvas()
    header(d, "能跑，不等于能扩展", "当前原型已经能移动和停格交互，但继续硬加建筑效果会快速变乱。")
    draw_board(d, 120, 285, 520)
    box(d, (780, 270, 1730, 845), fill=(27, 33, 46), r=28)
    items = [
        ("新建筑越来越多", RED),
        ("效果越来越多", ORANGE),
        ("UI / 动画 / 规则混在一起", PURPLE),
        ("AI 后续继续写，更容易沿着坏结构扩散", CYAN),
    ]
    y = 345
    for label, c in items:
        d.ellipse((835, y + 6, 865, y + 36), fill=c)
        text(d, (890, y), label, f=F_HEAD)
        y += 105
    for pts in [
        ((665, 510), (765, 405)),
        ((665, 540), (765, 510)),
        ((665, 570), (765, 620)),
        ((665, 600), (765, 720)),
    ]:
        line(d, pts[0], pts[1], fill=(110, 126, 150), width=4)
    text(d, (120, 910), "本期问题：不是“再加一个格子”，而是先让建筑系统可配置、可追踪、可测试。", fill=MUTED, f=F_SUB)
    save(im, "01-current-prototype-risk.png")


def slide_2():
    im, d = canvas()
    header(d, "错误方向：让事件暗中执行规则", "SO Event 可以解耦通知，但不应该让核心状态修改散落在监听器里。")
    box(d, (710, 240, 1210, 340), fill=PANEL, r=22)
    text(d, (960, 290), "玩家停到建筑", f=F_HEAD, anchor="mm")
    line(d, (960, 340), (960, 455), fill=RED, width=5)
    box(d, (660, 455, 1260, 555), fill=(54, 31, 40), outline=RED, r=22)
    text(d, (960, 505), "Raise FacilityTriggeredEvent", f=F_HEAD, anchor="mm")
    starts = [(960, 555), (960, 555), (960, 555)]
    ends = [(520, 690), (960, 690), (1400, 690)]
    labels = [("扣钱监听器\n改金币", RED), ("传送监听器\n改位置", ORANGE), ("UI 监听器\n弹窗", PURPLE)]
    for s, e, (label, color) in zip(starts, ends, labels):
        line(d, s, e, fill=color, width=5)
        box(d, (e[0] - 180, e[1], e[0] + 180, e[1] + 135), fill=PANEL_2, outline=color, r=20)
        text(d, (e[0], e[1] + 66), label, f=F_BODY, anchor="mm", align="center")
    risks = ["执行顺序不清楚", "监听器漏注册，规则就失效", "一个建筑到底做了什么很难追踪", "测试需要挂完整场景链路"]
    y = 875
    x = 250
    for r in risks:
        pill(d, (x, y, x + 340, y + 54), r, fill=(86, 44, 54), fg=TEXT)
        x += 365
    save(im, "02-event-driven-rule-trap.png")


def slide_3():
    im, d = canvas()
    header(d, "正确边界：SO 配置，流程执行，事件通知", "核心规则显式运行；SO Event 只负责把结果告诉 UI、动画、音效和日志。")
    nodes = [
        ("FacilityDefinition", "建筑配置 / 触发方式 / 效果列表", BLUE),
        ("FacilityInteractionResolver", "判断经过或停留是否触发，是否需要确认", CYAN),
        ("FacilityEffectExecutor", "按顺序执行效果，修改 GameState", GREEN),
        ("InteractionResult", "产出可展示、可测试的结果", ORANGE),
        ("SO Event Channels", "通知 UI / 动画 / 音效 / 日志", PURPLE),
    ]
    y = 245
    prev = None
    for title, sub, color in nodes:
        box(d, (520, y, 1400, y + 110), fill=PANEL, outline=color, r=24)
        text(d, (565, y + 26), title, f=F_HEAD, fill=TEXT)
        text(d, (565, y + 68), sub, f=F_SMALL, fill=MUTED)
        if prev:
            line(d, (960, prev), (960, y), fill=color, width=5)
        prev = y + 110
        y += 155
    text(d, (1550, 370), "规则可追踪", f=F_HEAD, fill=GREEN, anchor="mm")
    text(d, (1550, 435), "表现可替换", f=F_HEAD, fill=PURPLE, anchor="mm")
    text(d, (1550, 500), "测试可独立", f=F_HEAD, fill=CYAN, anchor="mm")
    save(im, "03-definition-resolver-executor-event.png")


def slide_4():
    im, d = canvas()
    header(d, "建筑 = 触发规则 + 效果列表", "一个建筑不必对应一个大脚本；它可以由可组合的效果资产拼出来。")
    cards = [
        ("奖励格", "StopAuto", ["AddMoney +200", "ShowFeedback"], GREEN),
        ("陷阱格", "PassAuto", ["AddMoney -100", "ShowFeedback"], RED),
        ("商店格", "StopConfirm", ["RequestChoice", "PurchaseFacility"], BLUE),
        ("传送格", "StopAuto", ["TeleportToTile", "ShowFeedback"], PURPLE),
    ]
    xs = [120, 560, 1000, 1440]
    for x, (name, trigger, effects, color) in zip(xs, cards):
        box(d, (x, 285, x + 360, 820), fill=PANEL, outline=color, r=24)
        d.rounded_rectangle((x, 285, x + 360, 365), radius=24, fill=color)
        text(d, (x + 180, 325), name, f=F_HEAD, fill=(12, 16, 24), anchor="mm")
        text(d, (x + 32, 420), "Trigger", f=F_SMALL, fill=MUTED)
        pill(d, (x + 32, 460, x + 250, 515), trigger, fill=(45, 55, 74), fg=TEXT)
        text(d, (x + 32, 575), "Effects", f=F_SMALL, fill=MUTED)
        yy = 620
        for effect in effects:
            d.rounded_rectangle((x + 32, yy, x + 328, yy + 58), radius=14, fill=PANEL_2, outline=BORDER)
            text(d, (x + 55, yy + 15), effect, f=F_CODE)
            yy += 75
    text(d, (960, 915), "后期加建筑：优先创建配置和组合效果；主流程不跟着膨胀。", f=F_SUB, fill=MUTED, anchor="mm")
    save(im, "04-facility-effect-composition.png")


def slide_5():
    im, d = canvas()
    header(d, "加玩法，不改主流程", "结尾演示可以用这张图收束：架构的价值是让新增建筑变成低风险操作。")
    text(d, (430, 250), "以前", f=F_TITLE, fill=RED, anchor="mm")
    text(d, (1450, 250), "现在", f=F_TITLE, fill=GREEN, anchor="mm")
    left = ["加新建筑", "改移动逻辑", "改交互逻辑", "改 UI", "旧建筑可能受影响"]
    right = ["创建 FacilityDefinition", "选择 Trigger Rule", "挂 EffectDefinition", "绑定表现资源", "运行测试"]
    y0 = 345
    for i, label in enumerate(left):
        y = y0 + i * 105
        box(d, (170, y, 690, y + 70), fill=(48, 30, 38), outline=RED, r=18)
        text(d, (430, y + 35), label, f=F_BODY, anchor="mm")
        if i < len(left) - 1:
            line(d, (430, y + 70), (430, y + 102), fill=RED, width=4)
    for i, label in enumerate(right):
        y = y0 + i * 105
        box(d, (1190, y, 1710, y + 70), fill=(28, 48, 40), outline=GREEN, r=18)
        text(d, (1450, y + 35), label, f=F_BODY, anchor="mm")
        if i < len(right) - 1:
            line(d, (1450, y + 70), (1450, y + 102), fill=GREEN, width=4)
    d.line((900, 320, 900, 875), fill=BORDER, width=3)
    text(d, (960, 920), "目标不是一次性完美，而是让 AI 后续改动有清晰落点。", f=F_SUB, fill=MUTED, anchor="mm")
    save(im, "05-add-new-facility-flow.png")


def contact_sheet():
    files = [
        "01-current-prototype-risk.png",
        "02-event-driven-rule-trap.png",
        "03-definition-resolver-executor-event.png",
        "04-facility-effect-composition.png",
        "05-add-new-facility-flow.png",
    ]
    thumbs = []
    for f in files:
        img = Image.open(OUT / f)
        img.thumbnail((560, 315))
        thumbs.append((f, img.copy()))
    im = Image.new("RGB", (1920, 1080), BG)
    d = ImageDraw.Draw(im)
    header(d, "视频图卡总览", "5 张图对应：现状、误区、边界、样例、结果。")
    positions = [(120, 240), (680, 240), (1240, 240), (400, 620), (960, 620)]
    for (name, img), (x, y) in zip(thumbs, positions):
        im.paste(img, (x, y))
        d.rectangle((x, y, x + img.width, y + img.height), outline=BORDER, width=2)
        text(d, (x, y + img.height + 18), name, f=F_SMALL, fill=MUTED)
    im.save(OUT / "00-contact-sheet.png", quality=95)


if __name__ == "__main__":
    slide_1()
    slide_2()
    slide_3()
    slide_4()
    slide_5()
    contact_sheet()
    print(f"Generated assets in {OUT}")
