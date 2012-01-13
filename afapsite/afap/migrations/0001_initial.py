# encoding: utf-8
import datetime
from south.db import db
from south.v2 import SchemaMigration
from django.db import models

class Migration(SchemaMigration):

    def forwards(self, orm):
        
        # Adding model 'Year'
        db.create_table('afap_year', (
            ('id', self.gf('django.db.models.fields.AutoField')(primary_key=True)),
            ('date', self.gf('afap.modelfields.YearField')(unique=True)),
            ('active', self.gf('django.db.models.fields.BooleanField')(default=False)),
            ('email_from', self.gf('django.db.models.fields.EmailField')(max_length=75)),
            ('read_only', self.gf('django.db.models.fields.BooleanField')(default=True)),
            ('validation_enabled', self.gf('django.db.models.fields.BooleanField')(default=True)),
        ))
        db.send_create_signal('afap', ['Year'])

        # Adding model 'Person'
        db.create_table('afap_person', (
            ('id', self.gf('django.db.models.fields.AutoField')(primary_key=True)),
            ('first_name', self.gf('django.db.models.fields.CharField')(max_length=255)),
            ('last_name', self.gf('django.db.models.fields.CharField')(max_length=255)),
            ('email_address', self.gf('django.db.models.fields.EmailField')(max_length=75)),
            ('phone_number', self.gf('django.contrib.localflavor.us.models.PhoneNumberField')(max_length=20, blank=True)),
        ))
        db.send_create_signal('afap', ['Person'])

        # Adding model 'ConstituencyGroup'
        db.create_table('afap_constituencygroup', (
            ('id', self.gf('django.db.models.fields.AutoField')(primary_key=True)),
            ('name', self.gf('django.db.models.fields.CharField')(max_length=255)),
        ))
        db.send_create_signal('afap', ['ConstituencyGroup'])

        # Adding model 'Organization'
        db.create_table('afap_organization', (
            ('id', self.gf('django.db.models.fields.AutoField')(primary_key=True)),
            ('user', self.gf('django.db.models.fields.related.ForeignKey')(to=orm['auth.User'], null=True, blank=True)),
            ('name', self.gf('django.db.models.fields.CharField')(max_length=255)),
            ('constituency_group', self.gf('django.db.models.fields.related.ForeignKey')(to=orm['afap.ConstituencyGroup'])),
            ('active', self.gf('django.db.models.fields.BooleanField')(default=True)),
        ))
        db.send_create_signal('afap', ['Organization'])

        # Adding model 'Application'
        db.create_table('afap_application', (
            ('id', self.gf('django.db.models.fields.AutoField')(primary_key=True)),
            ('organization', self.gf('django.db.models.fields.related.ForeignKey')(to=orm['afap.Organization'])),
            ('year', self.gf('afap.modelfields.YearField')()),
            ('president', self.gf('django.db.models.fields.related.ForeignKey')(related_name='president', null=True, to=orm['afap.Person'])),
            ('email_president', self.gf('django.db.models.fields.BooleanField')(default=True)),
            ('treasurer', self.gf('django.db.models.fields.related.ForeignKey')(related_name='treasurer', null=True, to=orm['afap.Person'])),
            ('email_treasurer', self.gf('django.db.models.fields.BooleanField')(default=True)),
            ('advisor', self.gf('django.db.models.fields.related.ForeignKey')(related_name='advisor', null=True, to=orm['afap.Person'])),
            ('email_advisor', self.gf('django.db.models.fields.BooleanField')(default=False)),
            ('members', self.gf('django.db.models.fields.IntegerField')(default=0)),
            ('dues', self.gf('django.db.models.fields.DecimalField')(default='0', max_digits=10, decimal_places=2)),
            ('new_members', self.gf('django.db.models.fields.IntegerField')(default=0)),
            ('purpose', self.gf('django.db.models.fields.TextField')(blank=True)),
            ('membership_requirements', self.gf('django.db.models.fields.TextField')(blank=True)),
            ('note', self.gf('django.db.models.fields.TextField')(blank=True)),
            ('last_modified', self.gf('django.db.models.fields.DateTimeField')(auto_now_add=True, blank=True)),
        ))
        db.send_create_signal('afap', ['Application'])

        # Adding model 'LineItemCategory'
        db.create_table('afap_lineitemcategory', (
            ('id', self.gf('django.db.models.fields.AutoField')(primary_key=True)),
            ('name', self.gf('django.db.models.fields.CharField')(max_length=255)),
            ('description', self.gf('django.db.models.fields.TextField')(blank=True)),
        ))
        db.send_create_signal('afap', ['LineItemCategory'])

        # Adding M2M table for field years on 'LineItemCategory'
        db.create_table('afap_lineitemcategory_years', (
            ('id', models.AutoField(verbose_name='ID', primary_key=True, auto_created=True)),
            ('lineitemcategory', models.ForeignKey(orm['afap.lineitemcategory'], null=False)),
            ('year', models.ForeignKey(orm['afap.year'], null=False))
        ))
        db.create_unique('afap_lineitemcategory_years', ['lineitemcategory_id', 'year_id'])

        # Adding model 'LineItem'
        db.create_table('afap_lineitem', (
            ('id', self.gf('django.db.models.fields.AutoField')(primary_key=True)),
            ('application', self.gf('django.db.models.fields.related.ForeignKey')(to=orm['afap.Application'])),
            ('category', self.gf('django.db.models.fields.related.ForeignKey')(to=orm['afap.LineItemCategory'])),
            ('description', self.gf('django.db.models.fields.CharField')(max_length=255)),
            ('amount', self.gf('django.db.models.fields.DecimalField')(max_digits=10, decimal_places=2)),
        ))
        db.send_create_signal('afap', ['LineItem'])

        # Adding model 'Approval'
        db.create_table('afap_approval', (
            ('id', self.gf('django.db.models.fields.AutoField')(primary_key=True)),
            ('approver', self.gf('django.db.models.fields.related.ForeignKey')(to=orm['afap.Person'])),
            ('application', self.gf('django.db.models.fields.related.ForeignKey')(to=orm['afap.Application'])),
            ('required', self.gf('django.db.models.fields.BooleanField')(default=True)),
            ('approved', self.gf('django.db.models.fields.BooleanField')(default=False)),
            ('key', self.gf('django.db.models.fields.CharField')(max_length=40)),
            ('approved_at', self.gf('django.db.models.fields.DateTimeField')(null=True, blank=True)),
        ))
        db.send_create_signal('afap', ['Approval'])


    def backwards(self, orm):
        
        # Deleting model 'Year'
        db.delete_table('afap_year')

        # Deleting model 'Person'
        db.delete_table('afap_person')

        # Deleting model 'ConstituencyGroup'
        db.delete_table('afap_constituencygroup')

        # Deleting model 'Organization'
        db.delete_table('afap_organization')

        # Deleting model 'Application'
        db.delete_table('afap_application')

        # Deleting model 'LineItemCategory'
        db.delete_table('afap_lineitemcategory')

        # Removing M2M table for field years on 'LineItemCategory'
        db.delete_table('afap_lineitemcategory_years')

        # Deleting model 'LineItem'
        db.delete_table('afap_lineitem')

        # Deleting model 'Approval'
        db.delete_table('afap_approval')


    models = {
        'afap.application': {
            'Meta': {'object_name': 'Application'},
            'advisor': ('django.db.models.fields.related.ForeignKey', [], {'related_name': "'advisor'", 'null': 'True', 'to': "orm['afap.Person']"}),
            'dues': ('django.db.models.fields.DecimalField', [], {'default': "'0'", 'max_digits': '10', 'decimal_places': '2'}),
            'email_advisor': ('django.db.models.fields.BooleanField', [], {'default': 'False'}),
            'email_president': ('django.db.models.fields.BooleanField', [], {'default': 'True'}),
            'email_treasurer': ('django.db.models.fields.BooleanField', [], {'default': 'True'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'last_modified': ('django.db.models.fields.DateTimeField', [], {'auto_now_add': 'True', 'blank': 'True'}),
            'members': ('django.db.models.fields.IntegerField', [], {'default': '0'}),
            'membership_requirements': ('django.db.models.fields.TextField', [], {'blank': 'True'}),
            'new_members': ('django.db.models.fields.IntegerField', [], {'default': '0'}),
            'note': ('django.db.models.fields.TextField', [], {'blank': 'True'}),
            'organization': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.Organization']"}),
            'president': ('django.db.models.fields.related.ForeignKey', [], {'related_name': "'president'", 'null': 'True', 'to': "orm['afap.Person']"}),
            'purpose': ('django.db.models.fields.TextField', [], {'blank': 'True'}),
            'treasurer': ('django.db.models.fields.related.ForeignKey', [], {'related_name': "'treasurer'", 'null': 'True', 'to': "orm['afap.Person']"}),
            'year': ('afap.modelfields.YearField', [], {})
        },
        'afap.approval': {
            'Meta': {'object_name': 'Approval'},
            'application': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.Application']"}),
            'approved': ('django.db.models.fields.BooleanField', [], {'default': 'False'}),
            'approved_at': ('django.db.models.fields.DateTimeField', [], {'null': 'True', 'blank': 'True'}),
            'approver': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.Person']"}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'key': ('django.db.models.fields.CharField', [], {'max_length': '40'}),
            'required': ('django.db.models.fields.BooleanField', [], {'default': 'True'})
        },
        'afap.constituencygroup': {
            'Meta': {'object_name': 'ConstituencyGroup'},
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'name': ('django.db.models.fields.CharField', [], {'max_length': '255'})
        },
        'afap.lineitem': {
            'Meta': {'object_name': 'LineItem'},
            'amount': ('django.db.models.fields.DecimalField', [], {'max_digits': '10', 'decimal_places': '2'}),
            'application': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.Application']"}),
            'category': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.LineItemCategory']"}),
            'description': ('django.db.models.fields.CharField', [], {'max_length': '255'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'})
        },
        'afap.lineitemcategory': {
            'Meta': {'object_name': 'LineItemCategory'},
            'description': ('django.db.models.fields.TextField', [], {'blank': 'True'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'name': ('django.db.models.fields.CharField', [], {'max_length': '255'}),
            'years': ('django.db.models.fields.related.ManyToManyField', [], {'to': "orm['afap.Year']", 'symmetrical': 'False'})
        },
        'afap.organization': {
            'Meta': {'object_name': 'Organization'},
            'active': ('django.db.models.fields.BooleanField', [], {'default': 'True'}),
            'constituency_group': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['afap.ConstituencyGroup']"}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'name': ('django.db.models.fields.CharField', [], {'max_length': '255'}),
            'user': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['auth.User']", 'null': 'True', 'blank': 'True'})
        },
        'afap.person': {
            'Meta': {'ordering': "['last_name', 'first_name']", 'object_name': 'Person'},
            'email_address': ('django.db.models.fields.EmailField', [], {'max_length': '75'}),
            'first_name': ('django.db.models.fields.CharField', [], {'max_length': '255'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'last_name': ('django.db.models.fields.CharField', [], {'max_length': '255'}),
            'phone_number': ('django.contrib.localflavor.us.models.PhoneNumberField', [], {'max_length': '20', 'blank': 'True'})
        },
        'afap.year': {
            'Meta': {'ordering': "['date']", 'object_name': 'Year'},
            'active': ('django.db.models.fields.BooleanField', [], {'default': 'False'}),
            'date': ('afap.modelfields.YearField', [], {'unique': 'True'}),
            'email_from': ('django.db.models.fields.EmailField', [], {'max_length': '75'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'read_only': ('django.db.models.fields.BooleanField', [], {'default': 'True'}),
            'validation_enabled': ('django.db.models.fields.BooleanField', [], {'default': 'True'})
        },
        'auth.group': {
            'Meta': {'object_name': 'Group'},
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'name': ('django.db.models.fields.CharField', [], {'unique': 'True', 'max_length': '80'}),
            'permissions': ('django.db.models.fields.related.ManyToManyField', [], {'to': "orm['auth.Permission']", 'symmetrical': 'False', 'blank': 'True'})
        },
        'auth.permission': {
            'Meta': {'ordering': "('content_type__app_label', 'content_type__model', 'codename')", 'unique_together': "(('content_type', 'codename'),)", 'object_name': 'Permission'},
            'codename': ('django.db.models.fields.CharField', [], {'max_length': '100'}),
            'content_type': ('django.db.models.fields.related.ForeignKey', [], {'to': "orm['contenttypes.ContentType']"}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'name': ('django.db.models.fields.CharField', [], {'max_length': '50'})
        },
        'auth.user': {
            'Meta': {'object_name': 'User'},
            'date_joined': ('django.db.models.fields.DateTimeField', [], {'default': 'datetime.datetime.now'}),
            'email': ('django.db.models.fields.EmailField', [], {'max_length': '75', 'blank': 'True'}),
            'first_name': ('django.db.models.fields.CharField', [], {'max_length': '30', 'blank': 'True'}),
            'groups': ('django.db.models.fields.related.ManyToManyField', [], {'to': "orm['auth.Group']", 'symmetrical': 'False', 'blank': 'True'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'is_active': ('django.db.models.fields.BooleanField', [], {'default': 'True'}),
            'is_staff': ('django.db.models.fields.BooleanField', [], {'default': 'False'}),
            'is_superuser': ('django.db.models.fields.BooleanField', [], {'default': 'False'}),
            'last_login': ('django.db.models.fields.DateTimeField', [], {'default': 'datetime.datetime.now'}),
            'last_name': ('django.db.models.fields.CharField', [], {'max_length': '30', 'blank': 'True'}),
            'password': ('django.db.models.fields.CharField', [], {'max_length': '128'}),
            'user_permissions': ('django.db.models.fields.related.ManyToManyField', [], {'to': "orm['auth.Permission']", 'symmetrical': 'False', 'blank': 'True'}),
            'username': ('django.db.models.fields.CharField', [], {'unique': 'True', 'max_length': '30'})
        },
        'contenttypes.contenttype': {
            'Meta': {'ordering': "('name',)", 'unique_together': "(('app_label', 'model'),)", 'object_name': 'ContentType', 'db_table': "'django_content_type'"},
            'app_label': ('django.db.models.fields.CharField', [], {'max_length': '100'}),
            'id': ('django.db.models.fields.AutoField', [], {'primary_key': 'True'}),
            'model': ('django.db.models.fields.CharField', [], {'max_length': '100'}),
            'name': ('django.db.models.fields.CharField', [], {'max_length': '100'})
        }
    }

    complete_apps = ['afap']
