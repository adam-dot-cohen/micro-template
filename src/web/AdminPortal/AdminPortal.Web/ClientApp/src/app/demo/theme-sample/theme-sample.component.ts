import { Component, HostBinding } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';

@Component({
  selector: 'app-theme-sample',
  templateUrl: './theme-sample.component.html',
  styleUrls: ['./theme-sample.component.scss']
})

export class ThemeSampleComponent {
  @HostBinding('class') public classes = 'app-main-content';

  public sampleForm = new FormGroup({
    checkBoxes: new FormGroup({
      check1: new FormControl(),
      check2: new FormControl(),
      check3: new FormControl()
    }),

    inputFields: new FormGroup({

    })
  });
}
