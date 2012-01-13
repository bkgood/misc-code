from reportlab.platypus import SimpleDocTemplate, Spacer, Paragraph, Table
from reportlab.lib.styles import getSampleStyleSheet
from reportlab.lib import colors
from reportlab import rl_config
from reportlab.lib.units import inch

from afap.models import Application, LineItem, LineItemCategory
from afap.util import sum_line_items

PAGE_HEIGHT = rl_config.defaultPageSize[1]
PAGE_WIDTH = rl_config.defaultPageSize[0]

styles = getSampleStyleSheet()

applications = [app for app in Application.objects.filter(year=2011,
    provisional=False, organization__constituency_group__name__startswith="Multi")
    if app.approved()]

def org_info(app, story):
    story.append(Paragraph("Organization Information", styles["Heading2"]))
    table_data = (
        ("President", Paragraph(str(app.president), styles["Normal"])),
        ("Treasurer", Paragraph(str(app.treasurer), styles["Normal"])),
        ("Advisor", Paragraph(str(app.advisor), styles["Normal"])),
        ("Number of members", app.members),
        ("Dues", "$%s" % (app.dues,)),
        ("Anticipated new members", app.new_members),
        #("Purpose", Paragraph(app.purpose, styles["Normal"])),
        #("Membership requirements", Paragraph(app.membership_requirements, styles["Normal"])),
        #("Application notes", Paragraph(app.note, styles["Normal"])),
        ("Account number", Paragraph(app.organization.account_number, styles["Normal"])),
    )
    story.append(Table(table_data, style=[('GRID', (0,0), (-1,-1), 1, colors.black)]))
    if app.purpose != '':
        story.append(Paragraph("Purpose", styles["Heading3"]))
        story.append(Paragraph(app.purpose, styles["Normal"]))
    if app.membership_requirements != '':
        story.append(Paragraph("Membership Requirements", styles["Heading3"]))
        story.append(Paragraph(app.membership_requirements, styles["Normal"]))
    if app.note != '':
        story.append(Paragraph("Application Notes", styles["Heading3"]))
        story.append(Paragraph(app.note, styles["Normal"]))

def budget_request(app, story):
    story.append(Paragraph("Budget Request", styles["Heading2"]))
    categories = []
    current_year = {}
    # XXX HACKS -- make current year categories always come first!
    for category in LineItemCategory.objects.filter(years__date=app.year).order_by('pk'):
        category_sum = sum_line_items(
            LineItem.objects.filter(application=app, category=category))
        # XXX HACKS
        if category.name.startswith("Current"):
            current_year['name'] = category.name
            current_year['sum'] = category_sum
            current_year['end_balance'] = category_sum - app.balance_forward - app.afap_income
            current_year['object'] = category
        else:
            categories.append({'name': category.name, 'sum': category_sum, 'object': category})
    total_sum = sum([x['sum'] for x in categories]) + current_year['end_balance']
    total_sum = total_sum or 0
    total_request = max(total_sum, 0)

    table_data = [
        ("Category", "Amount"),
        ("Balance forward", "$%s" % (app.balance_forward,)),
        ("AFAP Income", "$%s" % (app.afap_income,)),
        ("Current Year", "$%s" % (current_year['sum'],)),
        ("Current year end balance", "$%s" % (current_year['end_balance'],)),
        ("Projected %s expenses and income" % (app.year.range,),),
    ]

    for category in categories:
        table_data.append((category['name'], "$%s" % (category['sum'],)))
    print app, total_request
    table_data.append(("Sum", "$%s" % (total_sum,),),)
    table_data.append(("Request", "$%s" % (total_request,),))
    story.append(Table(table_data, repeatRows=1, style=[
        ('SPAN', (0,5), (1,5)),
        ('GRID', (0,0), (-1,-1), 1, colors.black)]))
    if current_year['object'].lineitem_set.filter(application=app).exists():
        budget_request_category(app, current_year['object'], story)
    for category in categories:
        if category['object'].lineitem_set.filter(application=app).exists():
            budget_request_category(app, category['object'], story)

def budget_request_category(app, category, story):
    story.append(Paragraph(category.name, styles["Heading3"]))
    table_data = [("Description", "Amount")]
    lineitems = LineItem.objects.filter(application=app, category=category)
    for lineitem in lineitems:
        if lineitem.income:
            table_data.append((Paragraph("%s (income)" % (lineitem.description,), styles['Normal']), "$%s" % -lineitem.amount))
        else:
            table_data.append((Paragraph(lineitem.description, styles["Normal"]), "$%s" % lineitem.amount))
    table_data.append(("Sum", "$%s" % sum_line_items(lineitems)))
    story.append(Table(table_data, repeatRows=1, style=[('GRID', (0,0), (-1,-1), 1, colors.black)]))

def approvals(app, story):
    story.append(Paragraph("Signatures", styles["Heading2"]))
    for approval in app.approval_set.filter(approved=True):
        if approval.approved_at.hour < 12:
            ampm = "a.m."
        else:
            ampm = "p.m."
        story.append(Paragraph("Approved by %s (%s) on %s at %s%s" % (
            unicode(approval.approver.name),
            unicode(approval.roles),
            approval.approved_at.date().strftime("%b %d, %Y"),
            approval.approved_at.time().strftime("%I:%M"),
            ampm), style=styles["Heading3"]))

def go(app):
#    doc = SimpleDocTemplate("%s.pdf" % app.organization.name, title=unicode(app))
    doc = SimpleDocTemplate("books/mcsc/%s.pdf" % app.organization.name, title=unicode(app))
    Story = []#[Spacer(1,2*inch)]
    style = styles["Heading1"]
    #for i in range(100):
    #    bogustext=  ("This is Paragraph number %s. "%i)*20
    #    p = Paragraph(bogustext, style)
    #    Story.append(p)
    #    Story.append(Spacer(1,0.2*inch))
    Story.append(Paragraph("%s" % (app.organization.name), style))
    org_info(app, Story)
    budget_request(app, Story)
    approvals(app, Story)
    doc.build(Story)

if __name__=="__main__":
    for app in applications:
        print app
        go(app)
