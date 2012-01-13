from django.conf.urls.defaults import *
from django.core import urlresolvers
from django.views.static import serve
from afap import views
import afap.forms
import afap.admin

from django.contrib import admin
from django.contrib import auth

urlpatterns = patterns('afap.views',
    (r'^admin/doc/', include('django.contrib.admindocs.urls')),
    (r'^admin/', include(afap.admin.admin_site.urls)),

    (r'^/?$', views.home),
    (r'^login/$', views.login),
    (r'^logout/$', views.logout),
    (r'^password_change/$', views.password_change),
    (r'^application/(?P<year>\d{4})/$', views.application),
    (r'^budgetrequest/(?P<year>\d{4})/$', views.budgetrequest),
    (r'^budgetrequest/(?P<year>\d{4})/(?P<category>\d+)$', views.budgetrequestcategory),
    (r'^signatures/(?P<year>\d{4})/$', views.signatures),
    (r'^archive/$', views.archive),
    (r'^approve/(?P<year>\d{4})/(?P<hash>[A-Fa-f0-9]{40})/$',
        views.approve),
    (r'^contact/$', views.contact),
    (r'^organization_select/$', views.organization_select),
    (r'^media/(?P<path>.*)$', serve, {
        'document_root': '/home/bill/src/afapsite/media',
        'show_indexes': True
    }),
)
