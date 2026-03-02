import { Component, OnInit } from '@angular/core';
import { Study } from '../../models/study.model';
import { StudyService } from '../../services/study';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-studies',
  imports: [FormsModule, CommonModule],
  templateUrl: './studies.html',
  styleUrl: './studies.scss',
})
export class Studies implements OnInit {

  studies: Study[] = [];
  selectedFile!: File;
  patientName = '';

  constructor(private studyService: StudyService) { }

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.studyService.getAll()
      .subscribe(res => this.studies = res);
  }

  onFileChange(event: any) {
    this.selectedFile = event.target.files[0];
  }

  upload() {
    this.studyService.create(this.selectedFile, this.patientName)
      .subscribe(() => {
        this.patientName = '';
        this.load();
      });
  }

  delete(id: number) {
    this.studyService.delete(id)
      .subscribe(() => this.load());
  }

  view(study: Study) {
    window.location.href =
      `http://127.0.0.1:3000/viewer?StudyInstanceUIDs=${study.studyInstanceUID}`;
  }
}